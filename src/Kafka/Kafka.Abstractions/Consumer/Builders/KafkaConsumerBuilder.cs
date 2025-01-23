using Confluent.Kafka;
using Kafka.Abstractions.Consumer.Inbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Consumer.Builders;

internal sealed class KafkaConsumerBuilder<TKey, TValue> : IKafkaConsumerAdditionalSelector<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _serviceCollection;

    public KafkaConsumerBuilder(string topicName, IConfiguration configuration, IServiceCollection serviceCollection)
    {
        _topicName = topicName;
        _configuration = configuration;
        _serviceCollection = serviceCollection;
    }

    public IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeKeyWith<T>()
        where T : class, IDeserializer<TKey>
    {
        _serviceCollection.AddKeyedSingleton<IDeserializer<TKey>, T>(_topicName);
        return this;
    }

    public IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeValueWith<T>()
        where T : class, IDeserializer<TValue>
    {
        _serviceCollection.AddKeyedSingleton<IDeserializer<TValue>, T>(_topicName);
        return this;
    }

    public IKafkaConsumerAdditionalSelector<TKey, TValue> WithInbox<TRepository>()
        where TRepository : class, IInboxRepository<TKey, TValue>
    {
        IConfigurationSection section = _configuration.GetSection("Inbox");
        _serviceCollection.AddOptions<InboxOptions>().Bind(section);

        _serviceCollection.AddScoped<IInboxRepository<TKey, TValue>, TRepository>();

        _serviceCollection.AddHostedService<InboxMessageProcessor<TKey, TValue>>(sp =>
            ActivatorUtilities.CreateInstance<InboxMessageProcessor<TKey, TValue>>(sp, _topicName));

        return this;
    }

    public IKafkaConsumerAdditionalSelector<TKey, TValue> WithInboxHandler<THandler>()
        where THandler : class, IKafkaConsumerHandler<TKey, TValue>
    {
        _serviceCollection.AddKeyedScoped<IKafkaConsumerHandler<TKey, TValue>>(_topicName, (sp, _) =>
            ActivatorUtilities.CreateInstance<THandler>(sp));

        return this;
    }

    public void Build()
    {
        _serviceCollection.AddHostedService<KafkaConsumer<TKey, TValue>>(sp =>
            ActivatorUtilities.CreateInstance<KafkaConsumer<TKey, TValue>>(sp, _topicName));
    }
}