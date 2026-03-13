namespace AuraFlow.Infrastructure.Messaging;

/// <summary>
/// Message queue interface for async processing (RabbitMQ compatible)
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Publish a message to the queue
    /// </summary>
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribe to messages from a queue
    /// </summary>
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a message with retry logic
    /// </summary>
    Task PublishWithRetryAsync<T>(string queueName, T message, int maxRetries = 3,
        TimeSpan? delayBetweenRetries = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Queue message wrapper for consistent messaging
/// </summary>
public class QueueMessage<T>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public T Data { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
