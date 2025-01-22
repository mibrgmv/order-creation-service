using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Kafka.Abstractions.Configuration;

internal class KafkaConfigurationBuilder : IKafkaConfigurationOptionsSelector, IKafkaConfigurationBuilder
{
    public KafkaConfigurationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IKafkaConfigurationBuilder WithOptions(IConfiguration configuration)
    {
        Services.AddOptions<KafkaConfigurationOptions>().Bind(configuration);
        return this;
    }
}