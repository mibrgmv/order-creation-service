using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Models.Models;

namespace OrderCreationService.Application.Abstractions.Persistence.Repositories;

public interface IOrderHistoryRepository
{
    Task AddItemAsync(OrderHistoryItem orderHistoryItem, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderHistoryItem> QueryItemsAsync(OrderHistoryQuery query, CancellationToken cancellationToken);
}