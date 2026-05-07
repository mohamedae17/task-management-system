using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IJwtService _jwt;
    private readonly ICurrentUserService _currentUser;
    private readonly IActivityLogger _activity;

    public LogoutCommandHandler(IJwtService jwt, ICurrentUserService currentUser, IActivityLogger activity)
    {
        _jwt = jwt;
        _currentUser = currentUser;
        _activity = activity;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.AllSessions)
        {
            if (_currentUser.UserId is not { } userId)
                throw new UnauthorizedException("Cannot revoke all sessions for an unauthenticated request.");

            await _jwt.RevokeAllForUserAsync(userId, "User logged out (all sessions)", cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _jwt.RevokeAsync(request.RefreshToken, request.IpAddress, "User logged out", cancellationToken);
        }

        if (_currentUser.UserId is { } id)
            await _activity.LogAsync(ActivityAction.Logout, "User", id, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
