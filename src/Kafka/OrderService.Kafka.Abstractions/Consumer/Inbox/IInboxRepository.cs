using Google.Protobuf;

namespace OrderService.Kafka.Abstractions.Consumer.Inbox;

public interface IInboxRepository<TKey, TValue>
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    Task AddOrUpdateAsync(InboxMessage<TKey, TValue> message, CancellationToken cancellationToken);

    IAsyncEnumerable<InboxMessage<TKey, TValue>> GetPendingMessagesAsync(CancellationToken cancellationToken, int? batchSize = null);
}