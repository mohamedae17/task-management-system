using MediatR;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly IEmailSender _email;

    public ForgotPasswordCommandHandler(IIdentityService identity, IEmailSender email)
    {
        _identity = identity;
        _email = email;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Intentionally do not reveal whether the email exists — same response either way to
        // avoid account enumeration. Only send a token if the user actually exists and is active.
        var user = await _identity.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive)
            return Unit.Value;

        var token = await _identity.GeneratePasswordResetTokenAsync(user);

        var body = $@"
<p>Hi {user.FullName},</p>
<p>A password reset was requested for your account. Submit the token below to <code>POST /api/auth/reset-password</code> along with your new password:</p>
<pre>UserId: {user.Id}
Token:  {token}</pre>
<p>If you did not request this reset, no further action is needed.</p>";

        await _email.SendAsync(new EmailMessage(
            To: user.Email!,
            Subject: "Reset your Task Management password",
            HtmlBody: body,
            ToDisplayName: user.FullName), cancellationToken);

        return Unit.Value;
    }
}
