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
    public static IServiceCollection AddPersistencePostgres(this IServiceCollection collection)
    {
        const string postgresKey = "Postgres";

        collection.AddOptions<PersistenceOptions>().BindConfiguration(postgresKey);

        collection.AddScoped<NpgsqlDataSource>(sp =>
        {
            PersistenceOptions options = sp.GetRequiredService<IOptionsSnapshot<PersistenceOptions>>().Value;

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(options.ConnectionString);

            dataSourceBuilder.MapEnum<OrderState>(pgName: "order_state");
            dataSourceBuilder.MapEnum<OrderHistoryItemKind>(pgName: "order_history_item_kind");

            return dataSourceBuilder.Build();
        });

        collection
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IOrderItemsRepository, OrderItemsRepository>()
            .AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();

        return collection;
    }

    public static IServiceCollection AddPersistenceMigrations(this IServiceCollection collection)
    {
        collection
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

        collection.AddHostedService<MigrationRunnerBackgroundService>();

        return collection;
    }
}