using OrderCreationService.Infrastructure.Kafka.Consumer.Models;

namespace OrderCreationService.Infrastructure.Kafka.Consumer;

public interface IKafkaConsumerHandler<TKey, TValue>
{
    ValueTask HandleAsync(KafkaConsumerMessage<TKey, TValue> message, CancellationToken cancellationToken);
}