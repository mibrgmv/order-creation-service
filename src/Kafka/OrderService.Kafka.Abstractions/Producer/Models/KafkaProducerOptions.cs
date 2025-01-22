namespace OrderService.Kafka.Abstractions.Producer.Models;

public sealed class KafkaProducerOptions
{
    public string Topic { get; set; } = string.Empty;
}