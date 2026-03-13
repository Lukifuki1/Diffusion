# AuraFlow Studio - Implementation Guide

## Overview

This guide documents the implementation of enterprise-grade components for AuraFlow Studio.

## 1. Database Layer (EF Core)

### Setup Steps:

```bash
# Install required packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Create initial migration
dotnet ef migrations add InitialCreate --project src/AuraFlow.Infrastructure
```

### Configuration:

Add to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuraFlowStudio;User Id=sa;Password=yourpassword;"
  }
}
```

### Usage Example:

```csharp
// In your service
public class PackageService
{
    private readonly IRepository<PackageVersion> _repository;
    
    public async Task AddPackageAsync(PackageVersion package)
    {
        await _repository.AddAsync(package);
    }
}
```

## 2. Caching Layer (Redis)

### Setup Steps:

```bash
# Install Redis
sudo apt-get install redis-server

# Start Redis
redis-server
```

### Configuration:

Add to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### Usage Example:

```csharp
public class ModelService
{
    private readonly ICacheService _cache;
    
    public async Task<ModelInfo> GetModelAsync(Guid modelId)
    {
        // Try cache first
        var cached = await _cache.GetAsync<ModelInfo>(
            $"model:{modelId}");
        
        if (cached != null)
            return cached;
        
        // Fetch from database
        var model = await _dbSet.FindAsync(modelId);
        
        // Cache for 24 hours
        await _cache.SetAsync(
            $"model:{modelId}", 
            model, 
            TimeSpan.FromHours(24));
        
        return model;
    }
}
```

## 3. Message Queue (RabbitMQ)

### Setup Steps:

```bash
# Install RabbitMQ
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

### Configuration:

Add to `appsettings.json`:
```json
{
  "RabbitMQ": {
    "Url": "amqp://localhost:5672"
  }
}
```

### Usage Example:

```csharp
public class ImageGenerationService
{
    private readonly IQueueService _queue;
    
    public async Task QueueImageGenerationAsync(GenerationRequest request)
    {
        await _queue.PublishWithRetryAsync(
            "image-generation", 
            new QueueMessage<GenerationRequest> 
            { 
                Data = request,
                CorrelationId = Guid.NewGuid().ToString()
            });
    }
}

// Consumer service
public class ImageGenerationConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _queue.SubscribeAsync<GenerationRequest>(
            "image-generation", 
            async (request) =>
            {
                // Process generation
                await GenerateImage(request);
            }, 
            stoppingToken);
    }
}
```

## 4. Background Jobs (Hangfire)

### Setup Steps:

```bash
# Install Hangfire
dotnet add package Hangfire.SqlServer
dotnet add package Hangfire.Core
```

### Configuration:

Add to `Startup.cs`:
```csharp
services.AddHangfire(config => 
    config.UseSqlServerStorage(
        configuration.GetConnectionString("Hangfire")));

services.AddHangfireServer();
```

### Usage Example:

```csharp
public class ScheduledJobsService
{
    private readonly IJobService _job;
    
    public async Task ScheduleDailyCleanupAsync()
    {
        await _job.AddRecurringJobAsync(
            "daily-cleanup", 
            "0 2 * * *", // Every day at 2 AM
            null,
            new RecurringJobOptions 
            { 
                Timezone = "Europe/Ljubljana" 
            });
    }
    
    public async Task QueueImageProcessingAsync(ImageData image)
    {
        await _job.EnqueueAsync(
            "image-processing", 
            image);
    }
}

// Job handler
public class ImageProcessingJob
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(ImageData data)
    {
        // Processing logic
    }
}
```

## 5. Monitoring & Logging (Sentry)

### Setup Steps:

```bash
# Install Sentry
dotnet add package Sentry.Serilog
dotnet add package Serilog.AspNetCore
```

### Configuration:

Add to `appsettings.json`:
```json
{
  "Sentry": {
    "Dsn": "https://your-dsn@sentry.io/123456",
    "EnableTracing": true,
    "TracesSampleRate": 0.1
  }
}
```

### Usage Example:

```csharp
public class GenerationService
{
    private readonly IMetricsService _metrics;
    
    public async Task<GenerationResult> GenerateAsync(GenerationRequest request)
    {
        using var scope = SentrySdk.StartTransaction(
            "image-generation", 
            "generation");
        
        try
        {
            await _metrics.IncrementCounterAsync("generations_total");
            
            var result = await PerformGeneration(request);
            
            await _metrics.RecordHistogramAsync(
                "generation_duration_ms", 
                result.DurationMs);
            
            return result;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            throw;
        }
    }
}
```

## 6. Rate Limiting Middleware

### Configuration:

Add to `Startup.cs`:
```csharp
app.UseMiddleware<RateLimitingMiddleware>();
```

### Usage Example:

```csharp
// API endpoint with rate limiting
[ApiController]
[Route("api/[controller]")]
public class GenerationController : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromBody] GenerationRequest request)
    {
        // This will be rate-limited automatically
        var result = await _service.GenerateAsync(request);
        return Ok(result);
    }
}
```

## 7. Authentication & Authorization (JWT)

### Configuration:

Add to `appsettings.json`:
```json
{
  "Auth": {
    "JwtSecret": "your-super-secret-key-min-32-chars",
    "Issuer": "AuraFlowStudio",
    "Audience": "AuraFlowUsers"
  }
}
```

### Usage Example:

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await ValidateUser(request);
        
        if (user == null)
            return Unauthorized();
        
        var token = await _auth.GenerateTokenAsync(user);
        
        return Ok(new 
        { 
            accessToken = token,
            expiresIn = "1h"
        });
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var result = await _auth.RefreshTokenAsync(request.Token);
        
        return Ok(result);
    }
}

// Protected endpoint
[Authorize(Roles = "Admin,User")]
[HttpGet("admin/dashboard")]
public async Task<IActionResult> AdminDashboard()
{
    // Only admins and users can access
}
```

## 8. Resilience Patterns (Polly)

### Configuration:

Add to `appsettings.json`:
```json
{
  "Resilience": {
    "MaxRetries": 3,
    "InitialRetryDelayMs": 1000,
    "FailureThreshold": 5,
    "CircuitBreakerTimeout": 30
  }
}
```

### Usage Example:

```csharp
public class ExternalApiService
{
    private readonly IResiliencePipeline _pipeline;
    
    public async Task<ApiResponse> CallExternalApiAsync()
    {
        // Retry with exponential backoff
        return await _pipeline.ExecuteWithRetryAsync(async () =>
        {
            // Circuit breaker protection
            return await _pipeline.ExecuteWithCircuitBreakerAsync(async () =>
            {
                // Timeout protection
                return await _pipeline.ExecuteWithTimeoutAsync(async () =>
                {
                    // Actual API call
                    using var client = new HttpClient();
                    return await client.GetAsync("https://api.example.com");
                }, TimeSpan.FromSeconds(30));
            });
        });
    }
}
```

## Testing Enterprise Components

### Unit Tests:

```csharp
public class CacheServiceTests
{
    [Fact]
    public async Task SetAndGet_CacheValue_Success()
    {
        var cache = new RedisCacheService(Mock.Of<IDistributedCache>());
        
        await cache.SetAsync("test", "value");
        var result = await cache.GetAsync<string>("test");
        
        Assert.Equal("value", result);
    }
}

public class RateLimitingTests
{
    [Fact]
    public async Task ExceedRateLimit_Returns429()
    {
        // Test rate limiting behavior
    }
}
```

### Integration Tests:

```csharp
[Collection("Sequential")]
public class RabbitMQIntegrationTests : IClassFixture<RabbitMqFixture>
{
    [Fact]
    public async Task PublishAndConsume_Message_Success()
    {
        // Test message queue integration
    }
}
```

## Performance Benchmarks

### Expected Performance:

- **Cache Hit Rate**: > 80% with proper configuration
- **API Response Time**: < 200ms for cached requests
- **Queue Throughput**: > 1000 messages/second
- **Job Processing**: Parallel processing supported

## Deployment Checklist

- [ ] Configure database connection string
- [ ] Deploy Redis instance
- [ ] Set up RabbitMQ server
- [ ] Configure Hangfire storage
- [ ] Add Sentry DSN
- [ ] Set JWT secret key (min 32 chars)
- [ ] Enable SSL/TLS
- [ ] Configure rate limits
- [ ] Set up monitoring alerts

## Troubleshooting

### Common Issues:

1. **Redis Connection Failed**: Check Redis is running and accessible
2. **RabbitMQ Queue Not Found**: Ensure queue names match in producer/consumer
3. **Hangfire Jobs Not Running**: Check Hangfire server is started
4. **JWT Token Invalid**: Verify secret key matches on both sides

### Logs:

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```
