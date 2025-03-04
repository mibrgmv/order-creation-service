using OrderCreationService.Application.Abstractions.Persistence;
using OrderCreationService.Application.Abstractions.Persistence.Repositories;

namespace OrderCreationService.Infrastructure.Persistence;

internal sealed class PersistenceContext : IPersistenceContext
{
    public PersistenceContext(
        IOrderRepository orderRepository,
        IProductRepository products,
        IOrderItemsRepository orderItems,
        IOrderHistoryRepository orderHistory)
    {
        Orders = orderRepository;
        Products = products;
        OrderItems = orderItems;
        OrderHistory = orderHistory;
    }

    public IOrderRepository Orders { get; }

    public IProductRepository Products { get; }

    public IOrderItemsRepository OrderItems { get; }

    public IOrderHistoryRepository OrderHistory { get; }
}