using Confluent.Kafka;

namespace OrderService.Kafka.Abstractions.Consumer.Models;

public class KafkaConsumerOptions
{
    public string Topic { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
}