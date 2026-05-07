using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly IActivityLogger _activity;

    public SetUserRolesCommandHandler(IIdentityService identity, IJwtService jwt, IActivityLogger activity)
    {
        _identity = identity;
        _jwt = jwt;
        _activity = activity;
    }

    public async Task<Unit> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _identity.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var result = await _identity.SetRolesAsync(user, request.Roles);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        // Roles are encoded into the JWT, so any old session has stale claims — revoke them.
        await _jwt.RevokeAllForUserAsync(user.Id, "Roles changed", cancellationToken);

        await _activity.LogAsync(
            ActivityAction.RoleChanged, "User", user.Id,
            metadata: new { Roles = request.Roles },
            cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
