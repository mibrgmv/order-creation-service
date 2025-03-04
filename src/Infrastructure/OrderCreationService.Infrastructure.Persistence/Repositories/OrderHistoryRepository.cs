using Npgsql;
using OrderCreationService.Application.Abstractions.Persistence.Queries;
using OrderCreationService.Application.Abstractions.Persistence.Repositories;
using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Application.Models.Payloads;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OrderCreationService.Infrastructure.Persistence.Repositories;

public class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddItemAsync(OrderHistoryItem orderHistoryItem, CancellationToken cancellationToken)
    {
        const string sql = """
        insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
        values (@order_id, @createdAt, @kind, @payload)
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string payloadJson = JsonSerializer.Serialize(orderHistoryItem.Payload);

        await using DbCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("order_id", orderHistoryItem.OrderId)
            .AddParameter("createdAt", orderHistoryItem.OrderHistoryItemCreatedAt)
            .AddParameter("kind", orderHistoryItem.OrderHistoryItemKind)
            .AddParameter("payload", JsonDocument.Parse(payloadJson));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<OrderHistoryItem> QueryItemsAsync(OrderHistoryQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select order_history_item_id, 
               order_id,
               order_history_item_created_at,
               order_history_item_kind,
               order_history_item_payload
        from order_history
        where (order_history_item_id > :cursor)
          and (cardinality(:ids) = 0 or order_history_item_id = any (:ids))
          and (:order_ids::bigint[] is null or cardinality(:order_ids) = 0 or order_id = any (:order_ids))
          and (:item_kind::order_history_item_kind is null or order_history_item_kind = :item_kind::order_history_item_kind)
        limit :page_size;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using DbCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("ids", query.Ids)
            .AddParameter("order_ids", query.OrderIds)
            .AddParameter("item_kind", query.ItemKind)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            OrderHistoryItemKind itemKind = await reader.GetFieldValueAsync<OrderHistoryItemKind>("order_history_item_kind", cancellationToken);

            string payloadJson = reader.GetString("order_history_item_payload");

            BasePayload payload = JsonSerializer.Deserialize<BasePayload>(payloadJson)
                                  ?? throw new InvalidOperationException("Payload deserialization error");

            yield return new OrderHistoryItem(
                OrderHistoryItemId: reader.GetInt64("order_history_item_id"),
                OrderId: reader.GetInt64("order_id"),
                OrderHistoryItemCreatedAt: await reader.GetFieldValueAsync<DateTimeOffset>("order_history_item_created_at", cancellationToken: cancellationToken),
                OrderHistoryItemKind: itemKind,
                Payload: payload);
        }
    }
}