using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Persistence.Repositories;

public interface IOrderItemsRepository
{
    Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken);

    Task SoftDeleteItemAsync(long orderId, long productId, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> QueryOrderItemsAsync(OrderItemQuery query, CancellationToken cancellationToken);
}