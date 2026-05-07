using MediatR;

namespace TaskManagement.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword) : IRequest<Unit>;
