using MediatR;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
