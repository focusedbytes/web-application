using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FocusedBytes.Api.Infrastructure.Middleware;

/// <summary>
/// Global exception handler that catches all unhandled exceptions and returns a standardized error response.
/// Implements IExceptionHandler for ASP.NET Core 8.0+ exception handling pipeline.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred while processing request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var statusCode = DetermineStatusCode(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception, statusCode),
            Detail = GetDetail(exception),
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Add exception details only in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;

            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    type = exception.InnerException.GetType().Name
                };
            }
        }

        // Add correlation ID if available
        if (httpContext.TraceIdentifier != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        return problemDetails;
    }

    private static int DetermineStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitle(Exception exception, int statusCode)
    {
        return exception switch
        {
            ArgumentNullException => "Bad Request",
            ArgumentException => "Bad Request",
            InvalidOperationException => "Bad Request",
            UnauthorizedAccessException => "Unauthorized",
            NotImplementedException => "Not Implemented",
            _ => "Internal Server Error"
        };
    }

    private string GetDetail(Exception exception)
    {
        // In production, don't expose internal error details
        if (!_environment.IsDevelopment())
        {
            return exception switch
            {
                ArgumentNullException => exception.Message,
                ArgumentException => exception.Message,
                InvalidOperationException => exception.Message,
                UnauthorizedAccessException => "You are not authorized to perform this action",
                _ => "An error occurred while processing your request. Please try again later."
            };
        }

        return exception.Message;
    }
}
