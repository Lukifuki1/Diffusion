using AuraFlow.Core.Common.Caching;

namespace AuraFlow.Api.Middleware;

/// <summary>
/// Middleware for API rate limiting to prevent abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ICacheService _cache;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(RequestDelegate next, 
        ILogger<RateLimitingMiddleware> logger,
        ICacheService cache,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        
        _options = new RateLimitOptions
        {
            RequestsPerMinute = config.GetValue<int>("RateLimits:RequestsPerMinute", 100),
            BurstSize = config.GetValue<int>("RateLimits:BurstSize", 20)
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var key = $"ratelimit:{clientId}";
        
        // Check if client exceeded rate limit
        var currentCount = await _cache.GetAsync<int>(key);
        
        if (currentCount == null)
            currentCount = 0;
        
        if (currentCount >= _options.RequestsPerMinute)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Append("X-RateLimit-Limit", _options.RequestsPerMinute.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", "0");
            
            await context.Response.WriteAsJsonAsync(new 
            {
                message = "Rate limit exceeded",
                retryAfter = 60 // seconds
            });
            return;
        }
        
        // Increment counter
        await _cache.SetAsync(key, currentCount + 1, TimeSpan.FromMinutes(1));
        
        context.Response.Headers.Append("X-RateLimit-Limit", _options.RequestsPerMinute.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", (currentCount + 1).ToString());
        
        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        // Try to get client ID from various sources
        return context.Connection.RemoteIpAddress?.ToString() ?? 
               context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
               "anonymous";
    }
}

/// <summary>
/// Rate limiting configuration options
/// </summary>
public class RateLimitOptions
{
    public int RequestsPerMinute { get; set; } = 100;
    public int BurstSize { get; set; } = 20;
}
