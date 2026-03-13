using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuraFlow.Api.Middleware;

/// <summary>
/// Request ID tracking middleware for distributed tracing
/// </summary>
public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestIdMiddleware> _logger;

    public RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or get request ID
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        context.Items["RequestId"] = requestId;
        context.Response.Headers.Append("X-Request-ID", requestId);

        _logger.LogDebug("Processing request {RequestId}", requestId);

        await _next(context);
    }
}

/// <summary>
/// JWT Authentication middleware configuration extension
/// </summary>
public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSecret = config["Auth:JwtSecret"] ?? "default-secret-key-min-32-chars";
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Auth:Issuer"],
                ValidAudience = config["Auth:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    _logger.LogError(context.Exception, "Token authentication failed");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        context.HttpContext.Items["UserId"] = userId;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        
        return services;
    }
}
