using AuraFlow.Core.Common.Monitoring;

namespace AuraFlow.Api.Middleware;

/// <summary>
/// Middleware for health check endpoints (/health, /ready, /live)
/// </summary>
public class HealthCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HealthCheckMiddleware> _logger;

    public HealthCheckMiddleware(RequestDelegate next, ILogger<HealthCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMetricsService metricsService)
    {
        if (context.Request.Path == "/health" || 
            context.Request.Path == "/ready" || 
            context.Request.Path == "/live")
        {
            await HandleHealthCheck(context, metricsService);
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleHealthCheck(HttpContext context, IMetricsService metricsService)
    {
        var healthStatus = await metricsService.GetHealthStatusAsync();
        
        // Set appropriate status codes
        if (context.Request.Path == "/health")
            context.Response.StatusCode = 200; // Always healthy
        else if (context.Request.Path == "/ready")
            context.Response.StatusCode = healthStatus.IsHealthy ? 200 : 503;
        else if (context.Request.Path == "/live")
            context.Response.StatusCode = 200; // Liveness check

        await context.Response.WriteAsJsonAsync(healthStatus);
    }
}

/// <summary>
/// Extension method to add health check middleware
/// </summary>
public static class HealthCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HealthCheckMiddleware>();
    }
}
