using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId?.ToString() ?? "anonymous";
        _logger.LogInformation("Handling {Request} for user {UserId}", requestName, userId);

        var response = await next();

        _logger.LogInformation("Handled  {Request} for user {UserId}", requestName, userId);
        return response;
    }
}
