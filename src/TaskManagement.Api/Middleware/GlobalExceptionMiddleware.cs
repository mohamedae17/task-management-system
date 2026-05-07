using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Exceptions;
using ApplicationValidationException = TaskManagement.Application.Common.Exceptions.ValidationException;

namespace TaskManagement.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, problem) = MapException(exception, context);

        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{Status} on {Method} {Path}: {Message}", status, context.Request.Method, context.Request.Path, exception.Message);

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private (int Status, ProblemDetails Problem) MapException(Exception ex, HttpContext context)
    {
        switch (ex)
        {
            case ApplicationValidationException ve:
                var vp = new ValidationProblemDetails(ve.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Instance = context.Request.Path
                };
                return ((int)HttpStatusCode.BadRequest, vp);

            case NotFoundException nf:
                return (StatusCodes.Status404NotFound, Build(StatusCodes.Status404NotFound, "Not Found", nf.Message, context));

            case UnauthorizedException ua:
                return (StatusCodes.Status401Unauthorized, Build(StatusCodes.Status401Unauthorized, "Unauthorized", ua.Message, context));

            case ForbiddenAccessException fa:
                return (StatusCodes.Status403Forbidden, Build(StatusCodes.Status403Forbidden, "Forbidden", fa.Message, context));

            case ConflictException cx:
                return (StatusCodes.Status409Conflict, Build(StatusCodes.Status409Conflict, "Conflict", cx.Message, context));

            case BusinessRuleException br:
                return (StatusCodes.Status422UnprocessableEntity, Build(StatusCodes.Status422UnprocessableEntity, "Business rule violation", br.Message, context));

            default:
                var detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.";
                return (StatusCodes.Status500InternalServerError, Build(StatusCodes.Status500InternalServerError, "Server error", detail, context));
        }
    }

    private static ProblemDetails Build(int status, string title, string detail, HttpContext context) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Instance = context.Request.Path
    };
}
