namespace OrderService.Kafka.Abstractions.Configuration;

public sealed class KafkaConfigurationOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
}