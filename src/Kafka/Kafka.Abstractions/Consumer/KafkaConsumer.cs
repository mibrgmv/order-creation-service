using Confluent.Kafka;
using Kafka.Abstractions.Configuration;
using Kafka.Abstractions.Consumer.Inbox;
using Kafka.Abstractions.Consumer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Abstractions.Consumer;

internal sealed class KafkaConsumer<TKey, TValue> : BackgroundService
{
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly KafkaConsumerOptions _options;
    private readonly IInboxRepository<TKey, TValue> _inboxRepository;
    private readonly ILogger<KafkaConsumer<TKey, TValue>> _logger;

    public KafkaConsumer(
        string topicName,
        IServiceProvider serviceProvider,
        IOptionsSnapshot<KafkaConsumerOptions> consumerOptions,
        IOptionsSnapshot<KafkaConfigurationOptions> configurationOptions,
        IInboxRepository<TKey, TValue> inboxRepository,
        ILogger<KafkaConsumer<TKey, TValue>> logger)
    {
        _options = consumerOptions.Get(topicName);
        _inboxRepository = inboxRepository;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configurationOptions.Value.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = _options.AutoOffsetReset,
            EnableAutoCommit = false,
        };

        IDeserializer<TKey> keyDeserializer = serviceProvider.GetRequiredKeyedService<IDeserializer<TKey>>(topicName);
        IDeserializer<TValue> valueDeserializer = serviceProvider.GetRequiredKeyedService<IDeserializer<TValue>>(topicName);

        _consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig)
            .SetKeyDeserializer(keyDeserializer)
            .SetValueDeserializer(valueDeserializer)
            .Build();

        _consumer.Subscribe(_options.Topic);
    }

    public override void Dispose()
    {
        _consumer.Close();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Run(() => ExecuteSingleAsync(stoppingToken), stoppingToken);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException or TaskCanceledException)
        {
            _logger.LogError(e, $"Error consuming messages from topic {_options.Topic}");
        }
    }

    private async Task ExecuteSingleAsync(CancellationToken stoppingToken)
    {
        ConsumeResult<TKey, TValue> result = _consumer.Consume(stoppingToken);

        var inboxMessage = new InboxMessage<TKey, TValue>(
            MessageId: default,
            MessageKey: result.Message.Key,
            MessageValue: result.Message.Value,
            CreatedAt: DateTime.UtcNow,
            ProcessedAt: null);

        await _inboxRepository.AddOrUpdateAsync(inboxMessage, stoppingToken);

        _consumer.Commit(result);
    }
}