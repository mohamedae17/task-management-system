using MediatR;

namespace TaskManagement.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Unit>;
