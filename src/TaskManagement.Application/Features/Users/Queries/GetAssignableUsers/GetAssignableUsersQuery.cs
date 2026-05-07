using MediatR;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetAssignableUsers;

public sealed record GetAssignableUsersQuery(string? Search = null) : IRequest<IReadOnlyList<UserSummaryDto>>;
