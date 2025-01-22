using Kafka.Abstractions.Producer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Producer.Builders;

public class KafkaProducerConfigurationSelector<TKey, TValue> : IKafkaProducerConfigurationSelector<TKey, TValue>
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaProducerConfigurationSelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaProducerAdditionalSelector<TKey, TValue> WithConfiguration(IConfiguration configuration)
    {
        string topicName = configuration.GetSection("Topic").Value
                           ?? throw new InvalidOperationException("Topic name undefined.");

        _serviceCollection.AddOptions<KafkaProducerOptions>(topicName).Bind(configuration);

        return new KafkaProducerBuilder<TKey, TValue>(topicName, configuration, _serviceCollection);
    }
}