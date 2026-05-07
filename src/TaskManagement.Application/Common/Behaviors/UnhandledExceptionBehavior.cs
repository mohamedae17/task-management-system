using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Application.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (ValidationException) { throw; }
        catch (NotFoundException) { throw; }
        catch (ForbiddenAccessException) { throw; }
        catch (UnauthorizedException) { throw; }
        catch (ConflictException) { throw; }
        catch (BusinessRuleException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}
