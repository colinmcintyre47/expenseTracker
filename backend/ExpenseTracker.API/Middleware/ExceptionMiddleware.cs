using System.Net;
using System.Text.Json;

namespace ExpenseTracker.API.Middleware;

/// <summary>
/// Global exception handler middleware.
///
/// Without this, unhandled exceptions return a 500 with a raw stack trace (dangerous in production).
/// With this, they return a clean JSON error response with an appropriate HTTP status code.
///
/// Exception type → HTTP status mapping:
/// - ArgumentException / InvalidOperationException → 400 Bad Request
/// - UnauthorizedAccessException → 401 Unauthorized
/// - KeyNotFoundException → 404 Not Found
/// - NotSupportedException → 400 Bad Request
/// - Everything else → 500 Internal Server Error (message hidden in production)
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException or InvalidOperationException or NotSupportedException
                => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException
                => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException
                => (HttpStatusCode.NotFound, exception.Message),
            _   => (HttpStatusCode.InternalServerError,
                    _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
