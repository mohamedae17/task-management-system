using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;
    private readonly IJwtService _jwt;
    private readonly IActivityLogger _activity;

    public ChangePasswordCommandHandler(
        IIdentityService identity,
        ICurrentUserService currentUser,
        IJwtService jwt,
        IActivityLogger activity)
    {
        _identity = identity;
        _currentUser = currentUser;
        _jwt = jwt;
        _activity = activity;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var user = await _identity.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var result = await _identity.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        await _jwt.RevokeAllForUserAsync(user.Id, "Password changed", cancellationToken);
        await _activity.LogAsync(ActivityAction.PasswordChanged, "User", user.Id, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
