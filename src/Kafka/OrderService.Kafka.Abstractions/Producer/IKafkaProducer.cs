using Confluent.Kafka;
using OrderService.Kafka.Abstractions.Producer.Models;

namespace OrderService.Kafka.Abstractions.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        KafkaProducerMessage<TKey, TValue> kafkaProducerMessage,
        CancellationToken cancellationToken);
}