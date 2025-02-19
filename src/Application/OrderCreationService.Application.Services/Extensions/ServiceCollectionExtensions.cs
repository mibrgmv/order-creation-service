using Microsoft.Extensions.DependencyInjection;
using OrderCreationService.Application.Contracts.Orders;
using OrderCreationService.Application.Services.Services;

namespace OrderCreationService.Application.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderService, OrderService>();
        return serviceCollection;
    }
}