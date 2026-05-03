using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shopbe.Web.Common.ExceptionHandling;

/// <summary>
/// Global exception handler that translates unhandled exceptions into RFC7807 ProblemDetails.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Request aborted by the client. Don't attempt to write a response.
            logger.LogInformation("Request aborted by client. Path: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(ex, "Unhandled exception occurred after the response started. Path: {Path}", context.Request.Path);
                throw;
            }

            var (problem, logLevel) = MapToProblemDetails(context, ex, env.IsDevelopment());
            logger.Log(logLevel, ex, "Request failed with {StatusCode}. Type: {Type}. Path: {Path}", problem.Status, problem.Type, context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }
    }

    private static (ProblemDetails problem, LogLevel logLevel) MapToProblemDetails(
        HttpContext context,
        Exception ex,
        bool isDevelopment)
    {
        var traceId = context.TraceIdentifier;

        var (status, title, type, errorCode, logLevel) = ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad request", "https://httpstatuses.com/400", "bad_request", LogLevel.Warning),
            FormatException => (StatusCodes.Status400BadRequest, "Bad request", "https://httpstatuses.com/400", "bad_request", LogLevel.Warning),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict", "https://httpstatuses.com/409", "conflict", LogLevel.Warning),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found", "https://httpstatuses.com/404", "not_found", LogLevel.Warning),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://httpstatuses.com/401", "unauthorized", LogLevel.Warning),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error", "https://httpstatuses.com/500", "internal_error", LogLevel.Error)
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            Instance = context.Request.Path
        };

        // In production keep messages generic; in dev include exception message.
        problem.Detail = isDevelopment ? ex.Message : (status >= 500 ? "An unexpected error occurred." : ex.Message);

        problem.Extensions["traceId"] = traceId;
        problem.Extensions["errorCode"] = errorCode;

        return (problem, logLevel);
    }
}



