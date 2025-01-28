using Confluent.Kafka;
using OrderCreationService.Infrastructure.Kafka.Producer.Models;

namespace OrderCreationService.Infrastructure.Kafka.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        KafkaProducerMessage<TKey, TValue> kafkaProducerMessage,
        CancellationToken cancellationToken);
}