using Confluent.Kafka;
using Kafka.Abstractions.Producer.Models;

namespace Kafka.Abstractions.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        KafkaProducerMessage<TKey, TValue> kafkaProducerMessage,
        CancellationToken cancellationToken);
}