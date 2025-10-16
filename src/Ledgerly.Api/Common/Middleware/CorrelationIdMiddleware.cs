using Serilog.Context;

namespace Ledgerly.Api.Common.Middleware;

/// <summary>
/// Middleware that ensures every HTTP request has a correlation ID for distributed tracing.
/// Adds X-Correlation-ID to both request logging context and response headers.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from header or generate new one
        var headerValue = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(headerValue)
            ? Guid.NewGuid().ToString()
            : headerValue;

        // Add to response headers
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Add to Serilog log context for structured logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
