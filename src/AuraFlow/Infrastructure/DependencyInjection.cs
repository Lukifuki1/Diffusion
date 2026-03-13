using AuraFlow.Core.Common.Caching;
using AuraFlow.Core.Common.Monitoring;
using AuraFlow.Core.Common.Auth;
using AuraFlow.Infrastructure.Messaging;
using AuraFlow.Infrastructure.Jobs;
using AuraFlow.Infrastructure.Persistence;
using AuraFlow.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuraFlow.Infrastructure;

/// <summary>
/// Dependency injection setup for all enterprise components
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddEnterpriseServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database & Persistence
        services.AddDbContext<AuraFlowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Caching (Redis + MemoryCache fallback)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options => 
                options.Configuration = redisConnectionString);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // Message Queue (RabbitMQ)
        var rabbitMqUrl = configuration.GetSection("RabbitMQ:Url").Value;
        if (!string.IsNullOrEmpty(rabbitMqUrl))
        {
            services.AddSingleton<IQueueService, RabbitQueueService>();
        }

        // Background Jobs (Hangfire)
        var storageConnectionString = configuration.GetConnectionString("Hangfire");
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            services.AddHangfire(config => 
                config.UseSqlServerStorage(storageConnectionString));
            services.AddHangfireServer();
        }

        // Monitoring & Metrics
        services.AddSingleton<IMetricsService, MetricsService>();

        // Authentication & Authorization
        services.AddScoped<IAuthService, AuthService>();

        // Resilience (Polly)
        var resilienceOptions = new ResilienceOptions();
        configuration.GetSection("Resilience").Bind(resilienceOptions);
        services.AddSingleton(resilienceOptions);
        services.AddScoped<IResiliencePipeline, ResiliencePipeline>();

        return services;
    }
}
