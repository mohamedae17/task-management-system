using MediatR;

namespace TaskManagement.Application.Features.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<Unit>;
