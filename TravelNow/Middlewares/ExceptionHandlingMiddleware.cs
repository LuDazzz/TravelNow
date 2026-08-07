using System.Security;
using Microsoft.AspNetCore.Mvc;
using TravelNow.Domain.Exceptions;

namespace TravelNow.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            SecurityException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing request {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                "Request failed with status code {StatusCode} for trace {TraceId}",
                statusCode,
                context.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = GetDetail(statusCode),
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad request",
        StatusCodes.Status401Unauthorized => "Authentication required",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Resource not found",
        _ => "Unexpected error"
    };

    private static string GetDetail(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "The request could not be processed.",
        StatusCodes.Status401Unauthorized => "Authentication is required to access this resource.",
        StatusCodes.Status403Forbidden => "You do not have permission to access this resource.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        _ => "An unexpected error occurred."
    };
}
