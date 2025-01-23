using Google.Protobuf.WellKnownTypes;
using Kafka.Abstractions.Producer.Outbox;
using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Abstractions.Requests;
using OrderCreationService.Application.Abstractions.Services;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Application.Models.Payloads;
using OrderCreationService.Application.Services.Exceptions;
using Orders.Kafka.Contracts;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace OrderCreationService.Application.Services.Services;

internal sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderItemsRepository _orderItemsRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly IOutboxRepository<OrderCreationKey, OrderCreationValue> _outboxRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IOrderItemsRepository orderItemsRepository,
        IOrderHistoryRepository orderHistoryRepository,
        IOutboxRepository<OrderCreationKey, OrderCreationValue> outboxRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _orderItemsRepository = orderItemsRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task<long[]> AddOrdersAsync(IReadOnlyCollection<AddOrderDto> orders, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        long[] orderIds = await _orderRepository.AddOrdersAsync(
            orders.Select(x =>
                new Order(
                    OrderId: default,
                    OrderState: OrderState.Created,
                    OrderCreatedAt: DateTime.UtcNow,
                    OrderCreatedBy: x.OrderCreatedBy))
                .ToList(),
            cancellationToken);

        foreach ((long id, AddOrderDto order) in orderIds.Zip(orders))
        {
            var payload = new AddOrderPayload(order.OrderCreatedBy);

            var item = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: id,
                OrderHistoryItemCreatedAt: DateTime.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.Created,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

            var key = new OrderCreationKey { OrderId = id };

            var value = new OrderCreationValue
            {
                OrderCreated = new OrderCreationValue.Types.OrderCreated
                {
                    OrderId = id,
                    CreatedAt = DateTime.UtcNow.ToTimestamp(),
                },
            };

            var outboxMessage = new OutboxMessage<OrderCreationKey, OrderCreationValue>(
                MessageId: default,
                MessageType: nameof(OrderCreationValue.Types.OrderCreated),
                MessageKey: key,
                MessageValue: value,
                CreatedAt: DateTime.UtcNow,
                ProcessedAt: null);

            await _outboxRepository.AddOrUpdateAsync(outboxMessage, cancellationToken);
        }

        transaction.Complete();
        return orderIds;
    }

    public async Task AddProductsToOrderAsync(
        long orderId,
        IReadOnlyCollection<AddProductToOrderDto> products,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot add products to an order of state: {order.OrderState}");

        foreach (AddProductToOrderDto product in products)
        {
            _ = await _productRepository.GetProductAsync(product.ProductId, cancellationToken);

            var orderItem = new OrderItem(
                OrderItemId: default,
                OrderId: order.OrderId,
                ProductId: product.ProductId,
                OrderItemQuantity: product.Quantity,
                OrderItemDeleted: false);

            await _orderItemsRepository.AddOrderItemAsync(orderItem, cancellationToken);

            var payload = new AddProductToOrderPayload(product.ProductId, product.Quantity);

            var historyItem = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: orderId,
                OrderHistoryItemCreatedAt: DateTime.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.ItemAdded,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(historyItem, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task RemoveProductsFromOrderAsync(long orderId, long[] productIds, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot remove products from an order of state: {order.OrderState}");

        foreach (long productId in productIds)
        {
            _ = await _productRepository.GetProductAsync(productId, cancellationToken);

            await _orderItemsRepository.SoftDeleteItemAsync(orderId, productId, cancellationToken);

            var payload = new RemoveProductFromOrderPayload(productId);

            var item = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: order.OrderId,
                OrderHistoryItemCreatedAt: DateTime.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.ItemRemoved,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(item, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task StartOrderProcessingAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot start order processing for order of state: {order.OrderState}, only of state {OrderState.Created}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Processing);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Processing, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTime.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        var key = new OrderCreationKey { OrderId = order.OrderId };

        var value = new OrderCreationValue
        {
            OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
            {
                OrderId = order.OrderId,
                StartedAt = DateTime.UtcNow.ToTimestamp(),
            },
        };

        var outboxMessage = new OutboxMessage<OrderCreationKey, OrderCreationValue>(
            MessageId: default,
            MessageType: nameof(OrderCreationValue.Types.OrderProcessingStarted),
            MessageKey: key,
            MessageValue: value,
            CreatedAt: DateTime.UtcNow,
            ProcessedAt: null);

        await _outboxRepository.AddOrUpdateAsync(outboxMessage, cancellationToken);

        transaction.Complete();
    }

    public async Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (order.OrderState != OrderState.Processing)
            throw new InvalidOrderStateException($"Cannot complete order of state: {order.OrderState}, only of state {OrderState.Processing}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Completed);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Completed, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTime.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        transaction.Complete();
    }

    public async Task CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot cancel order of state: {order.OrderState}, only of state {OrderState.Created}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Cancelled);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Cancelled, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTime.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        transaction.Complete();
    }

    public async Task UpdateOrderProcessingStatusAsync(long orderId, OrderProcessingStatus status, CancellationToken cancellationToken)
    {
        var payload = new OrderProcessingStatusPayload(status);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTime.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);
    }

    public async IAsyncEnumerable<Order> QueryOrdersAsync(OrderQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (Order order in _orderRepository.QueryOrdersAsync(query, cancellationToken))
            yield return order;
    }

    public async IAsyncEnumerable<OrderItem> QueryItemsAsync(OrderItemQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (OrderItem orderItem in _orderItemsRepository.QueryOrderItemsAsync(query, cancellationToken))
            yield return orderItem;
    }

    public async IAsyncEnumerable<OrderHistoryItem> QueryHistoryAsync(OrderHistoryQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (OrderHistoryItem historyItem in _orderHistoryRepository.QueryItemsAsync(query, cancellationToken))
            yield return historyItem;
    }
}