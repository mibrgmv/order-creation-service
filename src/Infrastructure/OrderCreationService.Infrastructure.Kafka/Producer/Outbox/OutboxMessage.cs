namespace OrderCreationService.Infrastructure.Kafka.Producer.Outbox;

public record OutboxMessage<TKey, TValue>(
    long MessageId,
    string MessageType,
    TKey MessageKey,
    TValue MessageValue,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);