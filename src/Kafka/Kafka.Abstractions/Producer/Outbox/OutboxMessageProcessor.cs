using Confluent.Kafka;
using Kafka.Abstractions.Producer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Abstractions.Producer.Outbox;

internal sealed class OutboxMessageProcessor<TKey, TValue> : BackgroundService
{
    private readonly string _topicName;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxMessageProcessor<TKey, TValue>> _logger;

    public OutboxMessageProcessor(
        string topicName,
        IServiceScopeFactory scopeFactory,
        IOptionsSnapshot<OutboxOptions> options,
        ILogger<OutboxMessageProcessor<TKey, TValue>> logger)
    {
        _topicName = topicName;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        TimeSpan pollingDelay = _options.PollingDelay;
        using var timer = new PeriodicTimer(pollingDelay);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ExecuteSingleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing outbox messages.");
            }
        }
    }

    private async Task ExecuteSingleAsync(CancellationToken stoppingToken)
    {
        int retryCount = _options.RetryCount;
        int batchSize = _options.BatchSize;

        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        IKafkaProducer<TKey, TValue> producer =
            scope.ServiceProvider.GetRequiredKeyedService<IKafkaProducer<TKey, TValue>>(_topicName);

        IOutboxRepository<TKey, TValue> outboxRepository =
            scope.ServiceProvider.GetRequiredService<IOutboxRepository<TKey, TValue>>();

        IEnumerable<OutboxMessage<TKey, TValue>> messages = outboxRepository
            .GetPendingMessagesAsync(stoppingToken, batchSize)
            .ToBlockingEnumerable(stoppingToken);

        foreach (OutboxMessage<TKey, TValue> outboxMessage in messages)
        {
            var message = new KafkaProducerMessage<TKey, TValue>(outboxMessage.MessageKey, outboxMessage.MessageValue);

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    DeliveryResult<TKey, TValue> result = await producer.ProduceAsync(message, stoppingToken);

                    if (result.Status == PersistenceStatus.Persisted)
                    {
                        _logger.LogInformation(
                            "Outbox message with {{message_id: {id}, message_type: {type}}} produced to Kafka.",
                            outboxMessage.MessageId,
                            outboxMessage.MessageType);

                        await outboxRepository.AddOrUpdateAsync(
                            outboxMessage with { ProcessedAt = DateTime.UtcNow },
                            stoppingToken);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error producing outbox message to Kafka. [attempt {attempt}/{retryCount}]",
                        attempt,
                        retryCount);
                }
            }
        }
    }
}