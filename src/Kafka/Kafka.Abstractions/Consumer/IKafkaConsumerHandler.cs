using Kafka.Abstractions.Consumer.Models;

namespace Kafka.Abstractions.Consumer;

public interface IKafkaConsumerHandler<TKey, TValue>
{
    ValueTask HandleAsync(KafkaConsumerMessage<TKey, TValue> message, CancellationToken cancellationToken);
}