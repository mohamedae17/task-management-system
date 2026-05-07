using MediatR;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
