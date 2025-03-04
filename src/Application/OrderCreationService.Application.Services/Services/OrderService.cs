using Google.Protobuf.WellKnownTypes;
using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Abstractions.Persistence.Repositories;
using OrderCreationService.Application.Contracts.Orders;
using OrderCreationService.Application.Contracts.Orders.Operations;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Application.Models.Payloads;
using OrderCreationService.Application.Services.Exceptions;
using OrderCreationService.Infrastructure.Kafka.Producer.Outbox;
using Orders.Kafka.Contracts;
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

    public async Task<long[]> AddOrdersAsync(
        IReadOnlyCollection<AddOrder.Request> orders,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        long[] orderIds = await _orderRepository.AddOrdersAsync(
            orders.Select(x =>
                new Order(
                    OrderId: default,
                    OrderState: OrderState.Created,
                    OrderCreatedAt: DateTimeOffset.UtcNow,
                    OrderCreatedBy: x.OrderCreatedBy))
                .ToList(),
            cancellationToken);

        foreach ((long id, AddOrder.Request order) in orderIds.Zip(orders))
        {
            var payload = new AddOrderPayload(order.OrderCreatedBy);

            var item = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: id,
                OrderHistoryItemCreatedAt: DateTimeOffset.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.Created,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

            var key = new OrderCreationKey { OrderId = id };

            var value = new OrderCreationValue
            {
                OrderCreated = new OrderCreationValue.Types.OrderCreated
                {
                    OrderId = id,
                    CreatedAt = DateTimeOffset.UtcNow.ToTimestamp(),
                },
            };

            var outboxMessage = new OutboxMessage<OrderCreationKey, OrderCreationValue>(
                MessageId: default,
                MessageType: nameof(OrderCreationValue.Types.OrderCreated),
                MessageKey: key,
                MessageValue: value,
                CreatedAt: DateTimeOffset.UtcNow,
                ProcessedAt: null);

            await _outboxRepository.AddOrUpdateAsync(outboxMessage, cancellationToken);
        }

        transaction.Complete();
        return orderIds;
    }

    public async Task AddProductsToOrderAsync(
        long orderId,
        IReadOnlyCollection<AddProductToOrder.Request> products,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var orderQuery = new OrderQuery([orderId], null, null, 0, 1);

        Order order = await _orderRepository
            .QueryOrdersAsync(orderQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OrderNotFoundException("Order not found.");

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot add products to an order of state: {order.OrderState}");

        foreach (AddProductToOrder.Request dto in products)
        {
            var productQuery = new ProductQuery([dto.ProductId], null, null, null, 0, 1);

            Product product = await _productRepository
                .QueryProductsAsync(productQuery, cancellationToken)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new ProductNotFoundException("Product not found.");

            var orderItem = new OrderItem(
                OrderItemId: default,
                OrderId: order.OrderId,
                ProductId: dto.ProductId,
                OrderItemQuantity: dto.Quantity,
                OrderItemDeleted: false);

            await _orderItemsRepository.AddOrderItemAsync(orderItem, cancellationToken);

            var payload = new AddProductToOrderPayload(dto.ProductId, dto.Quantity);

            var historyItem = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: orderId,
                OrderHistoryItemCreatedAt: DateTimeOffset.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.ItemAdded,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(historyItem, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task RemoveProductsFromOrderAsync(
        long orderId,
        long[] productIds,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var orderQuery = new OrderQuery([orderId], null, null, 0, 1);

        Order order = await _orderRepository
            .QueryOrdersAsync(orderQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OrderNotFoundException("Order not found.");

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot remove products from an order of state: {order.OrderState}");

        foreach (long productId in productIds)
        {
            var productQuery = new ProductQuery([productId], null, null, null, 0, 1);

            Product product = await _productRepository
                .QueryProductsAsync(productQuery, cancellationToken)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new ProductNotFoundException("Product not found.");

            await _orderItemsRepository.SoftDeleteItemAsync(orderId, productId, cancellationToken);

            var payload = new RemoveProductFromOrderPayload(productId);

            var item = new OrderHistoryItem(
                OrderHistoryItemId: default,
                OrderId: order.OrderId,
                OrderHistoryItemCreatedAt: DateTimeOffset.UtcNow,
                OrderHistoryItemKind: OrderHistoryItemKind.ItemRemoved,
                Payload: payload);

            await _orderHistoryRepository.AddItemAsync(item, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task StartOrderProcessingAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var orderQuery = new OrderQuery([orderId], null, null, 0, 1);

        Order order = await _orderRepository
            .QueryOrdersAsync(orderQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OrderNotFoundException("Order not found.");

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot start order processing for order of state: {order.OrderState}, only of state {OrderState.Created}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Processing);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Processing, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTimeOffset.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        var key = new OrderCreationKey { OrderId = order.OrderId };

        var value = new OrderCreationValue
        {
            OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
            {
                OrderId = order.OrderId,
                StartedAt = DateTimeOffset.UtcNow.ToTimestamp(),
            },
        };

        var outboxMessage = new OutboxMessage<OrderCreationKey, OrderCreationValue>(
            MessageId: default,
            MessageType: nameof(OrderCreationValue.Types.OrderProcessingStarted),
            MessageKey: key,
            MessageValue: value,
            CreatedAt: DateTimeOffset.UtcNow,
            ProcessedAt: null);

        await _outboxRepository.AddOrUpdateAsync(outboxMessage, cancellationToken);

        transaction.Complete();
    }

    public async Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var orderQuery = new OrderQuery([orderId], null, null, 0, 1);

        Order order = await _orderRepository
            .QueryOrdersAsync(orderQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OrderNotFoundException("Order not found.");

        if (order.OrderState != OrderState.Processing)
            throw new InvalidOrderStateException($"Cannot complete order of state: {order.OrderState}, only of state {OrderState.Processing}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Completed);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Completed, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTimeOffset.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        transaction.Complete();
    }

    public async Task CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var orderQuery = new OrderQuery([orderId], null, null, 0, 1);

        Order order = await _orderRepository
            .QueryOrdersAsync(orderQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new OrderNotFoundException("Order not found.");

        if (order.OrderState != OrderState.Created)
            throw new InvalidOrderStateException($"Cannot cancel order of state: {order.OrderState}, only of state {OrderState.Created}");

        var payload = new UpdateStatePayload(order.OrderState, OrderState.Cancelled);

        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Cancelled, cancellationToken);

        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTimeOffset.UtcNow,
            OrderHistoryItemKind.StateChanged,
            payload);

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);

        transaction.Complete();
    }

    public async Task UpdateOrderProcessingStatusAsync(
        long orderId,
        OrderProcessingStatus status,
        CancellationToken cancellationToken)
    {
        var item = new OrderHistoryItem(
            OrderHistoryItemId: default,
            orderId,
            DateTimeOffset.UtcNow,
            OrderHistoryItemKind.StateChanged,
            new BasePayload());

        await _orderHistoryRepository.AddItemAsync(item, cancellationToken);
    }

    public IAsyncEnumerable<Order> QueryOrdersAsync(
        QueryOrders.Request request,
        CancellationToken cancellationToken)
    {
        var query = new OrderQuery(
            Ids: request.Ids,
            OrderState: request.OrderState == OrderState.Unspecified ? null : request.OrderState,
            CreatedBy: request.CreatedBy,
            Cursor: request.Cursor,
            PageSize: request.PageSize);

        return _orderRepository.QueryOrdersAsync(query, cancellationToken);
    }

    public IAsyncEnumerable<OrderItem> QueryOrderItemsAsync(QueryOrderItems.Request request, CancellationToken cancellationToken)
    {
        var query = new OrderItemQuery(
            Ids: request.Ids,
            OrderIds: request.OrderIds,
            ProductIds: request.ProductIds,
            Deleted: request.Deleted,
            Cursor: request.Cursor,
            PageSize: request.PageSize);

        return _orderItemsRepository.QueryOrderItemsAsync(query, cancellationToken);
    }

    public IAsyncEnumerable<OrderHistoryItem> QueryOrderHistoryAsync(
        QueryOrderHistory.Request request,
        CancellationToken cancellationToken)
    {
        var query = new OrderHistoryQuery(
            Ids: request.Ids,
            OrderIds: request.OrderIds,
            ItemKind: request.ItemKind,
            Cursor: request.Cursor,
            PageSize: request.PageSize);

        return _orderHistoryRepository.QueryItemsAsync(query, cancellationToken);
    }
}