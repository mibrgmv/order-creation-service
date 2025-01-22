namespace Kafka.Abstractions.Producer.Outbox;

public interface IOutboxRepository<TKey, TValue>
{
    Task AddOrUpdateAsync(OutboxMessage<TKey, TValue> message, CancellationToken cancellationToken);

    IAsyncEnumerable<OutboxMessage<TKey, TValue>> GetPendingMessagesAsync(CancellationToken cancellationToken, int? batchSize = null);
}