namespace OrderCreationService.Infrastructure.Kafka.Consumer.Inbox;

public class InboxOptions
{
    public int BatchSize { get; set; }

    public TimeSpan PollingDelay { get; set; }

    public int RetryCount { get; set; }
}