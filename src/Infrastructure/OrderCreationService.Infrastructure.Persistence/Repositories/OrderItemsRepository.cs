using Npgsql;
using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Abstractions.Persistence.Repositories;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using System.Data;
using System.Runtime.CompilerServices;

namespace OrderCreationService.Infrastructure.Persistence.Repositories;

public class OrderItemsRepository : IOrderItemsRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemsRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken)
    {
        const string sql = """
        insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted) 
        values (:orderId, :productId, :quantity, :is_deleted)
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("orderId", orderItem.OrderId)
            .AddParameter("productId", orderItem.ProductId)
            .AddParameter("quantity", orderItem.OrderItemQuantity)
            .AddParameter("is_deleted", orderItem.OrderItemDeleted);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SoftDeleteItemAsync(long orderId, long productId, CancellationToken cancellationToken)
    {
        const string sql = """
        update order_items 
        set order_item_deleted = true 
        where order_id = :orderId 
          and product_id = :productId
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("orderId", orderId)
            .AddParameter("productId", productId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<OrderItem> QueryOrderItemsAsync(OrderItemQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select order_item_id, 
               order_id,
               product_id, 
               order_item_quantity,
               order_item_deleted
        from order_items
        where (order_item_id > :cursor)
          and (cardinality(:ids) = 0 or order_item_id = any (:ids))
          and (:order_ids::bigint[] is null or cardinality(:order_ids) = 0 or order_id = any (:order_ids::bigint[]))
          and (:product_ids::bigint[] is null or cardinality(:product_ids) = 0 or product_id = any (:product_ids::bigint[]))
          and (:deleted::boolean is null or order_item_deleted = :deleted::boolean)
        limit :page_size;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("ids", query.Ids)
            .AddParameter("order_ids", query.OrderIds)
            .AddParameter("product_ids", query.ProductIds)
            .AddParameter("deleted", query.Deleted)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderItem(
                reader.GetInt64("order_item_id"),
                reader.GetInt64("order_id"),
                reader.GetInt64("product_id"),
                reader.GetInt32("order_item_quantity"),
                reader.GetBoolean("order_item_deleted"));
        }
    }
}