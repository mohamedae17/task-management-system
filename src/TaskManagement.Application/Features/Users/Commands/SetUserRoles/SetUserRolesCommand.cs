using MediatR;

namespace TaskManagement.Application.Features.Users.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyList<string> Roles) : IRequest<Unit>;
