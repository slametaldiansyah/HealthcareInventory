using System.Net;
using System.Text.Json;
using FluentValidation;
using OrderInventory.Domain.Exceptions;

namespace OrderInventory.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                (object?)validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            InsufficientStockException insufficient => (
                HttpStatusCode.BadRequest,
                insufficient.Message,
                (object?)insufficient.Details.Select(d => new
                {
                    d.Sku,
                    d.Requested,
                    d.Available,
                    reason = "insufficient"
                }).ToList()),
            InvalidOrderException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                null),
            OrderConflictException => (
                HttpStatusCode.Conflict,
                exception.Message,
                null),
            ConcurrencyConflictException => (
                HttpStatusCode.Conflict,
                exception.Message,
                null),
            DuplicateKeyException => (
                HttpStatusCode.Conflict,
                exception.Message,
                null),
            NotFoundException => (
                HttpStatusCode.NotFound,
                exception.Message,
                null),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            status = (int)statusCode,
            title,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
