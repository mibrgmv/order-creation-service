using Microsoft.Extensions.DependencyInjection;

namespace OrderCreationService.Infrastructure.Kafka.Consumer.Builders;

internal sealed class KafkaConsumerKeySelector : IKafkaConsumerKeySelector
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