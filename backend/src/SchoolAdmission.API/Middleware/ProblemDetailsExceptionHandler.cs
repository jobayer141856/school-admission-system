using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SchoolAdmission.API.Middleware;

public sealed class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        var (status, title, detail) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}",
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException knf => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                knf.Message
            ),
            ArgumentException ae => (
                StatusCodes.Status400BadRequest,
                "Invalid request argument",
                ae.Message
            ),
            InvalidOperationException ioe => (
                StatusCodes.Status409Conflict,
                "Conflict",
                ioe.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again later."
            )
        };
    }
}
