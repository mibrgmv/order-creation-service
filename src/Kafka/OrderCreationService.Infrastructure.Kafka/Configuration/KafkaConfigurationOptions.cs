namespace OrderCreationService.Infrastructure.Kafka.Configuration;

public sealed class KafkaConfigurationOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
}