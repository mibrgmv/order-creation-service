namespace OrderService.Kafka.Abstractions.Consumer.Inbox;

public record InboxMessage<TKey, TValue>(
    long MessageId,
    TKey MessageKey,
    TValue MessageValue,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
