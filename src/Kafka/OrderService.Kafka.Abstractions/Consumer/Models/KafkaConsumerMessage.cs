namespace OrderService.Kafka.Abstractions.Consumer.Models;

public record KafkaConsumerMessage<TKey, TValue>(TKey Key, TValue Value);
