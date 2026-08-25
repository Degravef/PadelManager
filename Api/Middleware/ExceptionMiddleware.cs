using System.Text.Json;
using Core.Domain.Exceptions;
using FluentValidation;

namespace Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var errors = exception.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { Errors = errors }, JsonOptions));
    }

    private static Task HandleBusinessExceptionAsync(HttpContext context, BusinessException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { exception.Message }, JsonOptions));
    }

    private static Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsync(JsonSerializer.Serialize(
            new { Message = "An internal server error occurred." }, JsonOptions));
    }
}