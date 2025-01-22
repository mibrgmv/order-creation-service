using Confluent.Kafka;
using Google.Protobuf;

namespace OrderService.Kafka.Abstractions.Tools;

internal class ProtobufSerializer<T> : ISerializer<T>, IDeserializer<T> where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new MessageParser<T>(() => new T());

    public byte[] Serialize(T data, SerializationContext context)
    {
        return data.ToByteArray();
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return isNull
            ? throw new ArgumentNullException(nameof(data), "Null value found while deserializing Protobuf")
            : Parser.ParseFrom(data);
    }
}