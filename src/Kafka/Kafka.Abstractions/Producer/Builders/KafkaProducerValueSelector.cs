using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Producer.Builders;

internal sealed class KafkaProducerValueSelector<TKey> : IKafkaProducerValueSelector<TKey>
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaProducerValueSelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaProducerConfigurationSelector<TKey, TValue> WithValue<TValue>()
    {
        return new KafkaProducerConfigurationSelector<TKey, TValue>(_serviceCollection);
    }
}