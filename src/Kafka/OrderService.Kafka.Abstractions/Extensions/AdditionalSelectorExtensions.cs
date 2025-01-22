using Google.Protobuf;
using OrderService.Kafka.Abstractions.Consumer.Builders;
using OrderService.Kafka.Abstractions.Producer.Builders;
using OrderService.Kafka.Abstractions.Tools;

namespace OrderService.Kafka.Abstractions.Extensions;

public static class AdditionalSelectorExtensions
{
    public static IKafkaProducerAdditionalSelector<TKey, TValue> SerializeKeyWithProtobuf<TKey, TValue>(
        this IKafkaProducerAdditionalSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        selector.SerializeKeyWith<ProtobufSerializer<TKey>>();
        return selector;
    }

    public static IKafkaProducerAdditionalSelector<TKey, TValue> SerializeValueWithProtobuf<TKey, TValue>(
        this IKafkaProducerAdditionalSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        selector.SerializeValueWith<ProtobufSerializer<TValue>>();
        return selector;
    }

    public static IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeKeyWithProtobuf<TKey, TValue>(
        this IKafkaConsumerAdditionalSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        selector.DeserializeKeyWith<ProtobufSerializer<TKey>>();
        return selector;
    }

    public static IKafkaConsumerAdditionalSelector<TKey, TValue> DeserializeValueWithProtobuf<TKey, TValue>(
        this IKafkaConsumerAdditionalSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        selector.DeserializeValueWith<ProtobufSerializer<TValue>>();
        return selector;
    }
}