namespace OrderCreationService.Infrastructure.Kafka.Producer.Outbox;

public record OutboxMessage<TKey, TValue>(
    long MessageId,
    string MessageType,
    TKey MessageKey,
    TValue MessageValue,
    DateTime CreatedAt,
    DateTime? ProcessedAt);