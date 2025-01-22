namespace Kafka.Abstractions.Producer.Models;

public record KafkaProducerMessage<TKey, TValue>(TKey Key, TValue Value);