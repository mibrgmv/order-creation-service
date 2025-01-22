namespace Kafka.Abstractions.Consumer.Inbox;

public interface IInboxRepository<TKey, TValue>
{
    Task AddOrUpdateAsync(InboxMessage<TKey, TValue> message, CancellationToken cancellationToken);

    IAsyncEnumerable<InboxMessage<TKey, TValue>> GetPendingMessagesAsync(CancellationToken cancellationToken, int? batchSize = null);
}