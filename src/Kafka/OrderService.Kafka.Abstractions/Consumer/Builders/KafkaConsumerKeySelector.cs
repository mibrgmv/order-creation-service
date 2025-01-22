using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Kafka.Abstractions.Consumer.Builders;

internal class KafkaConsumerKeySelector : IKafkaConsumerKeySelector
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerKeySelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerValueSelector<TKey> WithKey<TKey>()
        where TKey : IMessage<TKey>, new()
    {
        return new KafkaConsumerValueSelector<TKey>(_serviceCollection);
    }
}