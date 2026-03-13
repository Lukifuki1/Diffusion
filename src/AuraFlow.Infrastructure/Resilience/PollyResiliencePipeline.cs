using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace AuraFlow.Infrastructure.Resilience;

/// <summary>
/// Polly-based resilience pipeline implementation
/// </summary>
public class PollyResiliencePipeline : IResiliencePipeline
{
    private readonly ResilienceOptions _options;
    private readonly ILogger<PollyResiliencePipeline> _logger;

    public PollyResiliencePipeline(ResilienceOptions options, 
        ILogger<PollyResiliencePipeline> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, 
        int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetries,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromMilliseconds(_options.InitialRetryDelayMs * 
                        Math.Pow(_options.RetryDelayMultiplier, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, 
                        "Retry {RetryCount}/{MaxRetries} after {TimeSpan}", 
                        retryCount, maxRetries, timeSpan);
                });

        try
        {
            return await retryPolicy.ExecuteAsync(action, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry policy failed after {MaxRetries} attempts", maxRetries);
            throw;
        }
    }

    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action,
        int failureThreshold = 5, TimeSpan resetTimeout = default, 
        CancellationToken cancellationToken = default)
    {
        if (resetTimeout == TimeSpan.Zero)
            resetTimeout = _options.CircuitBreakerTimeout;

        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: resetTimeout,
                onBreak: (exception, timeSpan) =>
                {
                    _logger.LogWarning(exception, 
                        "Circuit breaker opened after {Exceptions} failures for {Duration}",
                        failureThreshold, timeSpan);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker closed");
                });

        try
        {
            return await circuitBreakerPolicy.ExecuteAsync(action, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Circuit breaker is open, action skipped");
            throw;
        }
    }

    public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> action,
        TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var timeoutPolicy = Policy
            .TimeoutAsync(timeout);

        try
        {
            return await timeoutPolicy.ExecuteAsync(action, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Action timed out after {Timeout}", timeout);
            throw;
        }
    }
}
