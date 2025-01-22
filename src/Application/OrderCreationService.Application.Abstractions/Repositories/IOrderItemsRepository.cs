using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Repositories;

public interface IOrderItemsRepository
{
    Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken);

    Task SoftDeleteItemAsync(long orderId, long productId, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> QueryOrderItemsAsync(OrderItemQuery query, CancellationToken cancellationToken);
}