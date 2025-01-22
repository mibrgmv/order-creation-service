using Google.Protobuf;
using OrderService.Kafka.Abstractions.Consumer.Models;

namespace OrderService.Kafka.Abstractions.Consumer;

public interface IKafkaConsumerHandler<TKey, TValue>
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    ValueTask HandleAsync(KafkaConsumerMessage<TKey, TValue> message, CancellationToken cancellationToken);
}