using Confluent.Kafka;
using Kafka.Abstractions.Producer.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Producer.Builders;

internal sealed class KafkaProducerBuilder<TKey, TValue> : IKafkaProducerAdditionalSelector<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _serviceCollection;

    public KafkaProducerBuilder(string topicName, IConfiguration configuration, IServiceCollection serviceCollection)
    {
        _topicName = topicName;
        _configuration = configuration;
        _serviceCollection = serviceCollection;
    }

    public IKafkaProducerAdditionalSelector<TKey, TValue> SerializeKeyWith<T>()
        where T : class, ISerializer<TKey>
    {
        _serviceCollection.AddKeyedSingleton<ISerializer<TKey>, T>(_topicName);
        return this;
    }

    public IKafkaProducerAdditionalSelector<TKey, TValue> SerializeValueWith<T>()
        where T : class, ISerializer<TValue>
    {
        _serviceCollection.AddKeyedSingleton<ISerializer<TValue>, T>(_topicName);
        return this;
    }

    public IKafkaProducerAdditionalSelector<TKey, TValue> WithOutbox<TRepository>()
        where TRepository : class, IOutboxRepository<TKey, TValue>
    {
        IConfigurationSection section = _configuration.GetSection("Outbox");
        _serviceCollection.AddOptions<OutboxOptions>().Bind(section);

        _serviceCollection.AddScoped<IOutboxRepository<TKey, TValue>, TRepository>();

        _serviceCollection.AddHostedService<OutboxMessageProcessor<TKey, TValue>>(sp =>
            ActivatorUtilities.CreateInstance<OutboxMessageProcessor<TKey, TValue>>(sp, _topicName));

        return this;
    }

    public void Build()
    {
        _serviceCollection.AddKeyedScoped<IKafkaProducer<TKey, TValue>>(_topicName, (sp, _) =>
            ActivatorUtilities.CreateInstance<KafkaProducer<TKey, TValue>>(sp, _topicName));
    }
}
