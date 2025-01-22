using Kafka.Abstractions.Configuration;
using Kafka.Abstractions.Consumer.Builders;
using Kafka.Abstractions.Producer.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Abstractions.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddKafka(
        this IServiceCollection serviceCollection,
        Func<IKafkaConfigurationOptionsSelector, IKafkaConfigurationBuilder> func)
    {
        var builder = new KafkaConfigurationBuilder(serviceCollection);
        func.Invoke(builder);
        return serviceCollection;
    }

    public static IKafkaConfigurationBuilder AddProducer(
        this IKafkaConfigurationBuilder builder,
        Func<IKafkaProducerKeySelector, IKafkaProducerBuilder> func)
    {
        var selector = new KafkaProducerKeySelector(builder.Services);
        func.Invoke(selector).Build();
        return builder;
    }

    public static IKafkaConfigurationBuilder AddConsumer(
        this IKafkaConfigurationBuilder builder,
        Func<IKafkaConsumerKeySelector, IKafkaConsumerBuilder> func)
    {
        var selector = new KafkaConsumerKeySelector(builder.Services);
        func.Invoke(selector).Build();
        return builder;
    }
}