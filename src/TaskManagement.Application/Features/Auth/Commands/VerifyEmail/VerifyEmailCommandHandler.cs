using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Features.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IIdentityService _identity;

    public VerifyEmailCommandHandler(IIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _identity.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new BusinessRuleException("Verification link is invalid or has expired.");

        var result = await _identity.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        return Unit.Value;
    }
}
