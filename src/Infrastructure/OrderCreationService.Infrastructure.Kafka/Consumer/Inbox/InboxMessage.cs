namespace OrderCreationService.Infrastructure.Kafka.Consumer.Inbox;

public record InboxMessage<TKey, TValue>(
    long MessageId,
    TKey MessageKey,
    TValue MessageValue,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);
