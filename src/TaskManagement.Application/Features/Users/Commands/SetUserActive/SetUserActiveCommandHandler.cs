using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Users.Commands.SetUserActive;

public sealed class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly IJwtService _jwt;
    private readonly IActivityLogger _activity;

    public SetUserActiveCommandHandler(IIdentityService identity, IJwtService jwt, IActivityLogger activity)
    {
        _identity = identity;
        _jwt = jwt;
        _activity = activity;
    }

    public async Task<Unit> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _identity.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.IsActive == request.IsActive)
            return Unit.Value;

        await _identity.SetActiveAsync(user, request.IsActive, cancellationToken);

        if (!request.IsActive)
            await _jwt.RevokeAllForUserAsync(user.Id, "User deactivated", cancellationToken);

        await _activity.LogAsync(
            ActivityAction.Updated, "User", user.Id,
            metadata: new { IsActive = request.IsActive },
            cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
