using Kafka.Abstractions.Consumer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Consumer.Builders;

public class KafkaConsumerConfigurationSelector<TKey, TValue> : IKafkaConsumerConfigurationSelector<TKey, TValue>
{
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerConfigurationSelector(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerAdditionalSelector<TKey, TValue> WithConfiguration(IConfiguration configuration)
    {
        string topicName = configuration.GetSection("Topic").Value
                           ?? throw new InvalidOperationException("Topic name undefined.");

        _serviceCollection.AddOptions<KafkaConsumerOptions>(topicName).Bind(configuration);

        return new KafkaConsumerBuilder<TKey, TValue>(topicName, configuration, _serviceCollection);
    }
}