using Polly;
using Polly.Retry;

namespace AuraFlow.Infrastructure.Resilience;

/// <summary>
/// Resilience pipeline interface for retry and circuit breaker patterns
/// </summary>
public interface IResiliencePipeline
{
    /// <summary>
    /// Execute action with retry policy (exponential backoff)
    /// </summary>
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, 
        int maxRetries = 3, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute action with circuit breaker pattern
    /// </summary>
    Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action,
        int failureThreshold = 5, TimeSpan resetTimeout = default, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute action with timeout
    /// </summary>
    Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> action,
        TimeSpan timeout, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resilience options configuration
/// </summary>
public class ResilienceOptions
{
    public int MaxRetries { get; set; } = 3;
    public int InitialRetryDelayMs { get; set; } = 1000;
    public int RetryDelayMultiplier { get; set; } = 2;
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
