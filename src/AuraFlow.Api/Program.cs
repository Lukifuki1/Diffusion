using Microsoft.AspNetCore.Mvc;
using AuraFlow.Core.Services;
using AuraFlow.Infrastructure.Persistence;
using AuraFlow.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AuraFlow Studio API", 
        Version = "v1",
        Description = "Enterprise-grade AI Image & Video Generation Platform"
    });
});

// Register core services
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IGenerationService, GenerationService>();
builder.Services.AddScoped<ISettingsManager, SettingsManager>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Register infrastructure services
builder.Services.AddSingleton<IDatabaseContext, LiteDbDatabaseContext>();

// Add Redis caching (optional)
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "AuraFlow_";
    });
}

// Add Polly for resilience patterns
builder.Services.AddPolly();

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx => 
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Request.Headers["X-Api-Key"].ToString(), 
            factory => new FixedWindowRateLimiterOptions 
            { 
                PermitLimit = 100, 
                Window = TimeSpan.FromMinutes(1) 
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuraFlow Studio API v1");
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", async (IHealthCheckService service) => 
    await service.CheckHealthAsync());

app.Run();
