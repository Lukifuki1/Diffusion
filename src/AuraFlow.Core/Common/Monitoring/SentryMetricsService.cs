using System.Diagnostics;
using Sentry;

namespace AuraFlow.Core.Common.Monitoring;

/// <summary>
/// Sentry-based metrics service implementation
/// </summary>
public class SentryMetricsService : IMetricsService, IDisposable
{
    private readonly ILogger<SentryMetricsService> _logger;
    private readonly Stopwatch _stopwatch = new();
    private Dictionary<string, long> _counters = new();
    private Dictionary<string, double> _gauges = new();

    public SentryMetricsService(ILogger<SentryMetricsService> logger)
    {
        _logger = logger;
        
        // Initialize stopwatch for timing measurements
        _stopwatch.Start();
    }

    public async Task IncrementCounterAsync(string metricName, long value = 1, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_counters)
            {
                if (!_counters.ContainsKey(metricName))
                    _counters[metricName] = 0;

                _counters[metricName] += value;
            }

            // Send to Sentry for monitoring
            var tagsDict = tags ?? new Dictionary<string, string>();
            tagsDict["metric_name"] = metricName;
            
            SentrySdk.CaptureMessage(
                $"Counter incremented: {metricName}",
                scope =>
                {
                    foreach (var tag in tagsDict)
                        scope.SetTag(tag.Key, tag.Value);
                });

            _logger.LogDebug("Incremented counter {MetricName} by {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter: {MetricName}", metricName);
        }
    }

    public async Task SetGaugeAsync(string metricName, double value, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_gauges)
            {
                _gauges[metricName] = value;
            }

            var tagsDict = tags ?? new Dictionary<string, string>();
            tagsDict["metric_name"] = metricName;
            
            SentrySdk.CaptureMessage(
                $"Gauge set: {metricName} = {value}",
                scope =>
                {
                    foreach (var tag in tagsDict)
                        scope.SetTag(tag.Key, tag.Value);
                });

            _logger.LogDebug("Set gauge {MetricName} to {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting gauge: {MetricName}", metricName);
        }
    }

    public async Task RecordHistogramAsync(string metricName, double value, 
        Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagsDict = tags ?? new Dictionary<string, string>();
            tagsDict["metric_name"] = metricName;

            // Record as timing in Sentry
            using var timer = SentrySdk.StartTransaction(
                $"histogram:{metricName}", 
                "measurement",
                tagsDict.ToDictionary(k => k.Key, v => v.Value));

            await Task.Delay((int)value); // Simulate measurement
            
            _logger.LogDebug("Recorded histogram {MetricName} with value: {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording histogram: {MetricName}", metricName);
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = true;
            var details = new Dictionary<string, string>();

            // Check counters
            foreach (var counter in _counters)
            {
                if (counter.Value > 10000) // Threshold for high values
                {
                    details[$"high_counter_{counter.Key}"] = counter.Value.ToString();
                    isHealthy = false;
                }
            }

            // Check gauges
            foreach (var gauge in _gauges)
            {
                if (gauge.Value > 100) // Threshold for high values
                {
                    details[$"high_gauge_{gauge.Key}"] = gauge.Value.ToString();
                    isHealthy = false;
                }
            }

            return new HealthStatus
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "healthy" : "degraded",
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            
            return new HealthStatus
            {
                IsHealthy = false,
                Status = "error",
                Details = new Dictionary<string, string> 
                { { "error", ex.Message } },
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public void Dispose()
    {
        _stopwatch.Stop();
    }
}
