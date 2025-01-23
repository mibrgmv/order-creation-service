using Kafka.Abstractions.Consumer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Abstractions.Consumer.Inbox;

internal sealed class InboxMessageProcessor<TKey, TValue> : BackgroundService
{
    private readonly string _topicName;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly InboxOptions _options;
    private readonly ILogger<InboxMessageProcessor<TKey, TValue>> _logger;

    public InboxMessageProcessor(
        string topicName,
        IServiceScopeFactory scopeFactory,
        IOptionsSnapshot<InboxOptions> options,
        ILogger<InboxMessageProcessor<TKey, TValue>> logger)
    {
        _topicName = topicName;
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                _logger.LogError(ex, "Error while processing inbox messages.");
            }
        }
    }

    private async Task ExecuteSingleAsync(CancellationToken stoppingToken)
    {
        int retryCount = _options.RetryCount;
        int batchSize = _options.BatchSize;

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        IInboxRepository<TKey, TValue> inboxRepository =
            scope.ServiceProvider.GetRequiredService<IInboxRepository<TKey, TValue>>();

        IEnumerable<InboxMessage<TKey, TValue>> messages = inboxRepository
            .GetPendingMessagesAsync(stoppingToken, batchSize)
            .ToBlockingEnumerable(stoppingToken);

        IKafkaConsumerHandler<TKey, TValue> handler =
            scope.ServiceProvider.GetRequiredKeyedService<IKafkaConsumerHandler<TKey, TValue>>(_topicName);

        foreach (InboxMessage<TKey, TValue> inboxMessage in messages)
        {
            var message = new KafkaConsumerMessage<TKey, TValue>(inboxMessage.MessageKey, inboxMessage.MessageValue);

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    ValueTask task = handler.HandleAsync(message, stoppingToken);
                    await task;

                    if (task.IsCompletedSuccessfully)
                    {
                        _logger.LogInformation(
                            "Inbox message with {{message_id: {id}}} consumed by Kafka.",
                            inboxMessage.MessageId);

                        await inboxRepository.AddOrUpdateAsync(
                            inboxMessage with { ProcessedAt = DateTime.UtcNow },
                            stoppingToken);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error consuming inbox message from Kafka. [attempt {attempt}/{retryCount}]",
                        attempt,
                        retryCount);
                }
            }
        }
    }
}