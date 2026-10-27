using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Infrastructure;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}, TraceId={TraceId}",
                httpContext.Request.Method, httpContext.Request.Path, traceId);
        else
            logger.LogWarning("Request failed with {StatusCode}: {Reason}, TraceId={TraceId}",
                statusCode, exception.Message, traceId);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode >= 500 ? null : exception.Message, // 500-də daxili detal göstərilmir
                Extensions = { ["traceId"] = traceId }
            }
        });
    }
}