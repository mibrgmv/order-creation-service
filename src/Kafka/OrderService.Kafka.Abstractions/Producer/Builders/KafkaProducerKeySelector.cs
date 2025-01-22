using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Kafka.Abstractions.Producer.Builders;

internal class KafkaProducerKeySelector : IKafkaProducerKeySelector
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