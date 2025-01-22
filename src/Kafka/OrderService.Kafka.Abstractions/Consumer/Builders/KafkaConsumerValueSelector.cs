using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Kafka.Abstractions.Consumer.Builders;

internal class KafkaConsumerValueSelector<TKey> : IKafkaConsumerValueSelector<TKey>
    where TKey : IMessage<TKey>, new()
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerValueSelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>()
        where TValue : IMessage<TValue>, new()
    {
        return new KafkaConsumerConfigurationSelector<TKey, TValue>(_serviceCollection);
    }
}