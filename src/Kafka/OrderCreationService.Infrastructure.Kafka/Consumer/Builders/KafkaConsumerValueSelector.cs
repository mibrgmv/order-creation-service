using Microsoft.Extensions.DependencyInjection;

namespace OrderCreationService.Infrastructure.Kafka.Consumer.Builders;

internal sealed class KafkaConsumerValueSelector<TKey> : IKafkaConsumerValueSelector<TKey>
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerValueSelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>()
    {
        return new KafkaConsumerConfigurationSelector<TKey, TValue>(_serviceCollection);
    }
}