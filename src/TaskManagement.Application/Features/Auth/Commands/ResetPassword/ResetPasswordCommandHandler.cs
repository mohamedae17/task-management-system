using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly IActivityLogger _activity;

    public ResetPasswordCommandHandler(IIdentityService identity, IJwtService jwt, IActivityLogger activity)
    {
        _identity = identity;
        _jwt = jwt;
        _activity = activity;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identity.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new BusinessRuleException("Reset link is invalid or has expired.");

        var result = await _identity.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        // A password reset implicitly invalidates all existing sessions.
        await _jwt.RevokeAllForUserAsync(user.Id, "Password reset", cancellationToken);
        await _activity.LogAsync(ActivityAction.PasswordChanged, "User", user.Id, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
