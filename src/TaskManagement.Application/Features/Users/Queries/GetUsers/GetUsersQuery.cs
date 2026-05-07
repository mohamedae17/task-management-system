using MediatR;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQuery : PaginationRequest, IRequest<PaginatedList<UserDto>>
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
