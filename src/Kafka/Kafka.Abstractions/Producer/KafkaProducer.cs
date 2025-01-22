using Confluent.Kafka;
using Kafka.Abstractions.Configuration;
using Kafka.Abstractions.Producer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Abstractions.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly KafkaProducerOptions _options;
    private readonly ILogger<KafkaProducer<TKey, TValue>> _logger;

    public KafkaProducer(
        string topicName,
        IServiceProvider serviceProvider,
        IOptionsSnapshot<KafkaConfigurationOptions> kafkaOptions,
        IOptionsSnapshot<KafkaProducerOptions> producerOptions,
        ILogger<KafkaProducer<TKey, TValue>> logger)
    {
        _options = producerOptions.Get(topicName);
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
        };

        ISerializer<TKey> keySerializer = serviceProvider.GetRequiredKeyedService<ISerializer<TKey>>(topicName);
        ISerializer<TValue> valueSerializer = serviceProvider.GetRequiredKeyedService<ISerializer<TValue>>(topicName);

        _producer = new ProducerBuilder<TKey, TValue>(producerConfig)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
    }

    public async Task<DeliveryResult<TKey, TValue>> ProduceAsync(
        KafkaProducerMessage<TKey, TValue> kafkaProducerMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = new Message<TKey, TValue>
            {
                Key = kafkaProducerMessage.Key,
                Value = kafkaProducerMessage.Value,
            };

            return await _producer.ProduceAsync(_options.Topic, message, cancellationToken);
        }
        catch (ProduceException<TKey, TValue> e)
        {
            _logger.LogError(e, $"Error producing in topic {_options.Topic}");
            throw;
        }
    }
}