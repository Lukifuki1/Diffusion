namespace AuraFlow.Core.Common.Monitoring;

/// <summary>
/// Metrics service for application monitoring and telemetry
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Record a counter metric (e.g., number of generations)
    /// </summary>
    Task IncrementCounterAsync(string metricName, long value = 1, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Record a gauge metric (e.g., current queue size)
    /// </summary>
    Task SetGaugeAsync(string metricName, double value, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Record a histogram/metric with timing (e.g., generation duration)
    /// </summary>
    Task RecordHistogramAsync(string metricName, double value, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current health status of the application
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health check result
/// </summary>
public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, string>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
