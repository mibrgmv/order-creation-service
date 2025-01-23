using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Producer.Builders;

internal sealed class KafkaProducerKeySelector : IKafkaProducerKeySelector
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaProducerKeySelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaProducerValueSelector<TKey> WithKey<TKey>()
    {
        return new KafkaProducerValueSelector<TKey>(_serviceCollection);
    }
}