using OrderCreationService.Application.Abstractions.Persistence.Repositories;

namespace OrderCreationService.Application.Abstractions.Persistence;

public interface IPersistenceContext
{
    IOrderRepository Orders { get; }

    IProductRepository Products { get; }

    IOrderItemsRepository OrderItems { get; }

    IOrderHistoryRepository OrderHistory { get; }
}