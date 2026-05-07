using MediatR;

namespace TaskManagement.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(
    string? RefreshToken,
    bool AllSessions = false,
    string? IpAddress = null) : IRequest<Unit>;
