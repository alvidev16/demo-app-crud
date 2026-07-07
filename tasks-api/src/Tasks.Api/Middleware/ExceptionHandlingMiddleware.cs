using Microsoft.AspNetCore.Mvc;
using Tasks.Domain.Exceptions;

namespace Tasks.Api.Middleware;

/// <summary>Maps domain exceptions to RFC-7807 ProblemDetails (NFR-2), keeping controllers thin.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex is DomainException ? ex.Message : "An unexpected error occurred."
        };
        if (ex is ValidationException validation)
            problem.Extensions["errors"] = validation.Errors;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
