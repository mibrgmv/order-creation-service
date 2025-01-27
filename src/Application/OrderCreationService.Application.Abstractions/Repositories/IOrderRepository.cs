using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<long[]> AddOrdersAsync(IReadOnlyCollection<Order> orders, CancellationToken cancellationToken);

    Task UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken);

    IAsyncEnumerable<Order> QueryOrdersAsync(OrderQuery query, CancellationToken cancellationToken);
}