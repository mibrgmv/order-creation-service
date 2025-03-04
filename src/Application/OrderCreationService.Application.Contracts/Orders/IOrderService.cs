using OrderCreationService.Application.Contracts.Orders.Operations;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Contracts.Orders;

public interface IOrderService
{
    Task<long[]> AddOrdersAsync(IReadOnlyCollection<AddOrder.Request> orders, CancellationToken cancellationToken);

    Task AddProductsToOrderAsync(long orderId, IReadOnlyCollection<AddProductToOrder.Request> products, CancellationToken cancellationToken);

    Task RemoveProductsFromOrderAsync(long orderId, long[] productIds, CancellationToken cancellationToken);

    Task StartOrderProcessingAsync(long orderId, CancellationToken cancellationToken);

    Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken);

    Task CancelOrderAsync(long orderId, CancellationToken cancellationToken);

    Task UpdateOrderProcessingStatusAsync(long orderId, OrderProcessingStatus status, CancellationToken cancellationToken);

    IAsyncEnumerable<Order> QueryOrdersAsync(QueryOrders.Request request, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> QueryOrderItemsAsync(QueryOrderItems.Request request, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderHistoryItem> QueryOrderHistoryAsync(QueryOrderHistory.Request request, CancellationToken cancellationToken);
}