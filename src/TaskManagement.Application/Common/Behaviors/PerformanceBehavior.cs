using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Anything taking longer than this threshold is flagged in the logs.
    private const int SlowRequestThresholdMs = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
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
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request {Request} took {Elapsed} ms (user {UserId})",
                typeof(TRequest).Name, sw.ElapsedMilliseconds,
                _currentUser.UserId?.ToString() ?? "anonymous");
        }

        return response;
    }
}
