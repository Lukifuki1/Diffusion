using System.Net;
using System.Text.Json;

namespace AuraFlow.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            
            var response = context.Response;
            response.ContentType = "application/json";
            
            var errorResponse = new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = response.StatusCode,
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            if (ex is KeyNotFoundException)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = "Resource not found";
            }
            else if (ex is ArgumentException)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error = "Invalid argument";
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "Internal server error";
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(errorResponse, options);
            
            await response.WriteAsync(json);
        }
    }
}

public record ErrorResponse(
    string Error,
    int StatusCode,
    string Timestamp
);
