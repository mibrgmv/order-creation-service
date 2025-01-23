using Npgsql;
using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Infrastructure.Persistence.Exceptions;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using System.Data;
using System.Runtime.CompilerServices;

namespace OrderCreationService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Order> GetOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        const string sql = """
        select order_id,
               order_state,
               order_created_at,
               order_created_by
        from orders
        where order_id = :orderId 
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("orderId", orderId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            OrderState state = await reader.GetFieldValueAsync<OrderState>("order_state", cancellationToken);

            return new Order(
                reader.GetInt64("order_id"),
                state,
                reader.GetDateTime("order_created_at"),
                reader.GetString("order_created_by"));
        }

        throw new OrderNotFoundException("Order Not Found");
    }

    public async Task<long[]> AddOrdersAsync(IReadOnlyCollection<Order> orders, CancellationToken cancellationToken)
    {
        const string sql = """
        insert into orders(order_state, order_created_at, order_created_by) 
        select state, 
               created_at, 
               created_by
        from unnest(:states, :created_ats, :created_bys) as source(state, created_at, created_by)
        returning order_id;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("states", orders.Select(x => x.OrderState).ToArray())
            .AddParameter("created_ats", orders.Select(x => x.OrderCreatedAt).ToArray())
            .AddParameter("created_bys", orders.Select(x => x.OrderCreatedBy).ToArray());

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        IList<long> ids = [];

        while (await reader.ReadAsync(cancellationToken)) ids.Add(reader.GetInt64("order_id"));

        return ids.ToArray();
    }

    public async Task UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken)
    {
        const string sql = """
        update orders 
        set order_state = :newState 
        where order_id = :orderId
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("orderId", orderId)
            .AddParameter("newState", newState);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Order> QueryOrdersAsync(
        OrderQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select order_id,
               order_state,
               order_created_at,
               order_created_by
        from orders
        where (order_id > :cursor) 
          and (cardinality(:ids) = 0 or order_id = any (:ids)) 
          and (:created_by::text is null or order_created_by = :created_by::text) 
          and (:orderState::order_state is null or order_state = :orderState::order_state)
        limit :page_size;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("ids", query.Ids)
            .AddParameter("created_by", query.CreatedBy)
            .AddParameter("orderState", query.State)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            OrderState state = await reader.GetFieldValueAsync<OrderState>("order_state", cancellationToken);

            yield return new Order(
                reader.GetInt64("order_id"),
                state,
                reader.GetDateTime("order_created_at"),
                reader.GetString("order_created_by"));
        }
    }
}