using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;

namespace AuraFlow.Api.Controllers;

/// <summary>
/// REST API controller for health checks and monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HealthStatus>> CheckHealth()
    {
        var status = await _healthCheckService.CheckHealthAsync();
        
        if (status.Status == HealthStatusType.Healthy)
            return Ok(status);
            
        return StatusCode(503, status);
    }

    /// <summary>
    /// Detailed health check with all components
    /// </summary>
    [HttpGet("detailed")]
    public async Task<ActionResult<DetailedHealthStatus>> CheckHealthDetailed()
    {
        var status = await _healthCheckService.CheckHealthDetailedAsync();
        
        if (status.Status == HealthStatusType.Healthy)
            return Ok(status);
            
        return StatusCode(503, status);
    }

    /// <summary>
    /// Readiness check for load balancers
    /// </summary>
    [HttpGet("ready")]
    public async Task<ActionResult<bool>> CheckReadiness()
    {
        var isReady = await _healthCheckService.IsReadyAsync();
        
        if (isReady)
            return Ok(true);
            
        return StatusCode(503, new { ready = false });
    }

    /// <summary>
    /// Liveness check for Kubernetes
    /// </summary>
    [HttpGet("live")]
    public ActionResult<bool> CheckLiveness()
    {
        // Simple liveness check - always returns true if service is running
        return Ok(true);
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<PerformanceMetrics>> GetMetrics()
    {
        var metrics = await _healthCheckService.GetPerformanceMetricsAsync();
        return Ok(metrics);
    }

    /// <summary>
    /// Get system resources usage
    /// </summary>
    [HttpGet("resources")]
    public async Task<ActionResult<SystemResources>> GetResources()
    {
        var resources = await _healthCheckService.GetSystemResourcesAsync();
        return Ok(resources);
    }
}

/// <summary>
/// Health status response model
/// </summary>
public record HealthStatus(
    string Status,
    DateTime Timestamp,
    string Version,
    List<HealthComponent> Components
);

/// <summary>
/// Detailed health status with more information
/// </summary>
public record DetailedHealthStatus(
    string Status,
    DateTime Timestamp,
    string Version,
    TimeSpan ResponseTimeMs,
    Dictionary<string, HealthComponentDetails> Components,
    List<string> Warnings
);

/// <summary>
/// Performance metrics response model
/// </summary>
public record PerformanceMetrics(
    double RequestsPerSecond,
    double AverageResponseTimeMs,
    double ErrorRatePercentage,
    int ActiveConnections,
    long TotalRequests,
    DateTime LastUpdated
);

/// <summary>
/// System resources response model
/// </summary>
public record SystemResources(
    double CpuUsagePercentage,
    double MemoryUsagePercentage,
    long AvailableMemoryBytes,
    long TotalMemoryBytes,
    double DiskUsagePercentage,
    long FreeDiskSpaceBytes,
    long TotalDiskSpaceBytes
);

/// <summary>
/// Health component model
/// </summary>
public record HealthComponent(
    string Name,
    string Status,
    string? Message
);

/// <summary>
/// Detailed health component model
/// </summary>
public record HealthComponentDetails(
    string Name,
    string Status,
    string? Message,
    TimeSpan ResponseTimeMs,
    Dictionary<string, object>? Details
);
