using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using OrderService.Kafka.Abstractions.Consumer.Inbox;

namespace OrderService.Kafka.Abstractions.Consumer.Builders;

public interface IKafkaConsumerBuilder
{
    void Build();
}

public interface IKafkaConsumerKeySelector
{
    IKafkaConsumerValueSelector<TKey> WithKey<TKey>()
        where TKey : IMessage<TKey>, new();
}

public interface IKafkaConsumerValueSelector<TKey>
    where TKey : IMessage<TKey>, new()
{
    IKafkaConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>()
        where TValue : IMessage<TValue>, new();
}

public interface IKafkaConsumerConfigurationSelector<TKey, TValue>
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    IKafkaConsumerAdditionalSelector<TKey, TValue> WithConfiguration(IConfiguration configuration);
}

public interface IKafkaConsumerAdditionalSelector<TKey, TValue> : IKafkaConsumerBuilder
    where TValue : IMessage<TValue>, new()
    where TKey : IMessage<TKey>, new()
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