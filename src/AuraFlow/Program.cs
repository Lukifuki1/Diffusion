using AuraFlow.Middlewares;
using AuraFlow.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the dependency injection container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AuraFlow Studio API", Version = "v1" });
});

// Register core services
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddSingleton<ISecretsManager>(new SecretsManager());
builder.Services.AddSingleton<ISettingsManager>(new SettingsManager());

// Register infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure CORS for OpenWebUI integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOpenWebUI",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
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

app.UseCors("AllowOpenWebUI");
app.UseMiddleware<HealthCheckMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
