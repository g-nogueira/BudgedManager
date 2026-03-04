using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace MonthlyBudget.Infrastructure.Middleware;
public sealed class RequestLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggerMiddleware> _logger;
    public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
    { _next = next; _logger = logger; }
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        _logger.LogInformation("{Method} {Path} → {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode);
    }
}

