using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Infrastructure.Persistence.Repositories;

namespace OrderCreationService.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<NpgsqlDataSource>(sp =>
            {
                PersistenceOptions options = sp.GetRequiredService<IOptionsSnapshot<PersistenceOptions>>().Value;
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(options.ConnectionString);
                dataSourceBuilder.MapEnum<OrderState>(pgName: "order_state");
                dataSourceBuilder.MapEnum<OrderHistoryItemKind>(pgName: "order_history_item_kind");
                return dataSourceBuilder.Build();
            });

        serviceCollection
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                {
                    PersistenceOptions options = sp.GetRequiredService<IOptionsSnapshot<PersistenceOptions>>().Value;
                    return options.ConnectionString;
                })
                .ScanIn(typeof(IAssemblyMarker).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        serviceCollection
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IOrderItemsRepository, OrderItemsRepository>()
            .AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();

        return serviceCollection;
    }
}