using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCreationService.Infrastructure.Kafka.Configuration;

public interface IKafkaConfigurationBuilder
{
    IServiceCollection Services { get; }
}

public interface IKafkaConfigurationOptionsSelector
{
    IKafkaConfigurationBuilder WithOptions(IConfiguration configuration);
}