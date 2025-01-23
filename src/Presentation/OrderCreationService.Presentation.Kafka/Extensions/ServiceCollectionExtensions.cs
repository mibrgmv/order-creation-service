using Kafka.Abstractions.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderCreationService.Presentation.Kafka.ConsumerHandlers;
using OrderCreationService.Presentation.Kafka.Repositories;
using Orders.Kafka.Contracts;

namespace OrderCreationService.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        const string configurationKey = "Kafka:Configuration";
        const string producerKey = "Kafka:Producers:OrderCreation";
        const string consumerKey = "Kafka:Consumers:OrderProcessing";

        serviceCollection.AddKafka(builder => builder
            .WithOptions(configuration.GetSection(configurationKey))
            .AddProducer(selector => selector
                .WithKey<OrderCreationKey>()
                .WithValue<OrderCreationValue>()
                .WithConfiguration(configuration.GetSection(producerKey))
                .SerializeKeyWithProtobuf()
                .SerializeValueWithProtobuf()
                .WithOutbox<OrderCreationOutboxRepository>())
            .AddConsumer(selector => selector
                .WithKey<OrderProcessingKey>()
                .WithValue<OrderProcessingValue>()
                .WithConfiguration(configuration.GetSection(consumerKey))
                .DeserializeKeyWithProtobuf()
                .DeserializeValueWithProtobuf()
                .WithInbox<OrderProcessingInboxRepository>()
                .WithInboxHandler<OrderProcessingConsumerHandler>()));

        return serviceCollection;
    }
}