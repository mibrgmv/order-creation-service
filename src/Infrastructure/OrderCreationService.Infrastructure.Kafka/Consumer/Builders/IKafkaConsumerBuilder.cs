using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OrderCreationService.Infrastructure.Kafka.Consumer.Inbox;

namespace OrderCreationService.Infrastructure.Kafka.Consumer.Builders;

public interface IKafkaConsumerBuilder
{
    void Build();
}

public interface IKafkaConsumerKeySelector
{
    IKafkaConsumerValueSelector<TKey> WithKey<TKey>();
}

public interface IKafkaConsumerValueSelector<TKey>
{
    IKafkaConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>();
}

public interface IKafkaConsumerConfigurationSelector<TKey, TValue>
{
    IKafkaConsumerAdditionalSelector<TKey, TValue> WithConfiguration(IConfiguration configuration);
}

public interface IKafkaConsumerAdditionalSelector<TKey, TValue> : IKafkaConsumerBuilder
{
    public IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeKeyWith<T>()
        where T : class, IDeserializer<TKey>;

    public IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeValueWith<T>()
        where T : class, IDeserializer<TValue>;

    public IKafkaConsumerAdditionalSelector<TKey, TValue> WithInbox<TRepository>()
        where TRepository : class, IInboxRepository<TKey, TValue>;

    public IKafkaConsumerAdditionalSelector<TKey, TValue> WithInboxHandler<THandler>()
        where THandler : class, IKafkaConsumerHandler<TKey, TValue>;
}