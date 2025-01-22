using OrderService.Kafka.Abstractions.Consumer.Models;

namespace OrderService.Kafka.Abstractions.Consumer;

public interface IKafkaConsumerHandler<TKey, TValue>
{
    ValueTask HandleAsync(KafkaConsumerMessage<TKey, TValue> message, CancellationToken cancellationToken);
}