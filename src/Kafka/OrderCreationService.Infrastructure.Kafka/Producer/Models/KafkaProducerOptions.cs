namespace OrderCreationService.Infrastructure.Kafka.Producer.Models;

public sealed class KafkaProducerOptions
{
    public string Topic { get; set; } = string.Empty;
}