using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Consumer.Builders;

internal class KafkaConsumerKeySelector : IKafkaConsumerKeySelector
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerKeySelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerValueSelector<TKey> WithKey<TKey>()
    {
        return new KafkaConsumerValueSelector<TKey>(_serviceCollection);
    }
}