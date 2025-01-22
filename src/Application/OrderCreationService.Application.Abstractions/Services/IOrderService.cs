using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Services;

public interface IOrderService
{
    Task<long[]> AddOrdersAsync(IReadOnlyCollection<AddOrderDto> orders, CancellationToken cancellationToken);

    Task AddProductsToOrderAsync(long orderId, IReadOnlyCollection<AddProductToOrderDto> products, CancellationToken cancellationToken);

    Task RemoveProductsFromOrderAsync(long orderId, long[] productIds, CancellationToken cancellationToken);

    Task StartOrderProcessingAsync(long orderId, CancellationToken cancellationToken);

    Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken);

    Task CancelOrderAsync(long orderId, CancellationToken cancellationToken);

    Task UpdateOrderProcessingStatusAsync(long orderId, OrderProcessingStatus status, CancellationToken cancellationToken);

    IAsyncEnumerable<Order> QueryOrdersAsync(OrderQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> QueryItemsAsync(OrderItemQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderHistoryItem> QueryHistoryAsync(OrderHistoryQuery query, CancellationToken cancellationToken);
}