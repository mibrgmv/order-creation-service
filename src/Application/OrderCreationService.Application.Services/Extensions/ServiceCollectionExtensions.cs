using Microsoft.Extensions.DependencyInjection;
using OrderCreationService.Application.Contracts.Orders;
using OrderCreationService.Application.Contracts.Products;
using OrderCreationService.Application.Services.Services;

namespace OrderCreationService.Application.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IProductService, ProductService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();
        return serviceCollection;
    }
}