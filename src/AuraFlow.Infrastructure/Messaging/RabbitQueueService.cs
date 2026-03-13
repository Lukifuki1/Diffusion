using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuraFlow.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of the queue service
/// </summary>
public class RabbitQueueService : IQueueService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitQueueService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitQueueService(
        IConfiguration configuration,
        ILogger<RabbitQueueService> logger)
    {
        _logger = logger;
        
        var url = configuration.GetSection("RabbitMQ:Url").Value ?? "amqp://localhost";
        var factory = new ConnectionFactory { Uri = new Uri(url) };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateChannel();
        
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var queueKey = $"queue:{queueName}";
        
        _channel.QueueDeclare(
            queue: queueKey,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = JsonSerializer.Serialize(message, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(body);

        _channel.BasicPublish(
            exchange: "",
            routingKey: queueKey,
            basicProperties: null,
            body: bytes);

        _logger.LogInformation("Message published to {Queue}", queueName);
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, 
        CancellationToken cancellationToken = default)
    {
        var queueKey = $"queue:{queueName}";
        
        _channel.QueueDeclare(
            queue: queueKey,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(body, _jsonOptions);
                
                if (message != null)
                {
                    await handler(message);
                    
                    // Acknowledge message after successful processing
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Queue}", queueName);
                
                // Reject and requeue on error
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueKey,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Subscribed to {Queue}", queueName);
    }

    public async Task PublishWithRetryAsync<T>(string queueName, T message, int maxRetries = 3,
        TimeSpan? delayBetweenRetries = null, CancellationToken cancellationToken = default)
    {
        var retryDelay = delayBetweenRetries ?? TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await PublishAsync(queueName, message, cancellationToken);
                return; // Success
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogWarning(ex, "Retry {Attempt}/{Max} for queue {Queue}", 
                    i + 1, maxRetries, queueName);
                
                await Task.Delay(retryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to publish message after {maxRetries} retries");
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
