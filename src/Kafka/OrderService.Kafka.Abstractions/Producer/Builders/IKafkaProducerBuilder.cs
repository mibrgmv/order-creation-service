using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OrderService.Kafka.Abstractions.Producer.Outbox;

namespace OrderService.Kafka.Abstractions.Producer.Builders;

public interface IKafkaProducerBuilder
{
    void Build();
}

public interface IKafkaProducerKeySelector
{
    IKafkaProducerValueSelector<TKey> WithKey<TKey>();
}

public interface IKafkaProducerValueSelector<TKey>
{
    IKafkaProducerConfigurationSelector<TKey, TValue> WithValue<TValue>();
}

public interface IKafkaProducerConfigurationSelector<TKey, TValue>
{
    IKafkaProducerAdditionalSelector<TKey, TValue> WithConfiguration(IConfiguration configuration);
}

public interface IKafkaProducerAdditionalSelector<TKey, TValue> : IKafkaProducerBuilder
{
    IKafkaProducerAdditionalSelector<TKey, TValue> SerializeKeyWith<T>()
        where T : class, ISerializer<TKey>;

    IKafkaProducerAdditionalSelector<TKey, TValue> SerializeValueWith<T>()
        where T : class, ISerializer<TValue>;

    IKafkaProducerAdditionalSelector<TKey, TValue> WithOutbox<TRepository>()
        where TRepository : class, IOutboxRepository<TKey, TValue>;
}
