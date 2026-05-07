using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IApplicationDbContext db, IIdentityService identity, IMapper mapper)
    {
        _db = db;
        _identity = identity;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking();

        if (request.IsActive is { } active)
            query = query.Where(u => u.IsActive == active);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.Email!, $"%{s}%") ||
                EF.Functions.Like(u.FirstName, $"%{s}%") ||
                EF.Functions.Like(u.LastName, $"%{s}%"));
        }

        query = (request.SortBy?.ToLowerInvariant(), request.SortDescending) switch
        {
            ("email", true)        => query.OrderByDescending(u => u.Email),
            ("email", false)       => query.OrderBy(u => u.Email),
            ("lastlogin", true)    => query.OrderByDescending(u => u.LastLoginAt),
            ("lastlogin", false)   => query.OrderBy(u => u.LastLoginAt),
            ("createdat", true)    => query.OrderByDescending(u => u.CreatedAt),
            ("createdat", false)   => query.OrderBy(u => u.CreatedAt),
            _                      => query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
        };

        var paged = await PaginatedList<Domain.Entities.AppUser>.CreateAsync(
            query, request.PageNumber, request.PageSize, cancellationToken);

        // Optionally filter by role after pagination via UserManager. Filtering server-side
        // would require joining UserRoles + Roles which is doable but noisy; the typical
        // page sizes here (≤ 200) make a per-user role lookup cheap enough.
        var dtos = new List<UserDto>(paged.Items.Count);
        foreach (var u in paged.Items)
        {
            var roles = await _identity.GetRolesAsync(u);
            if (!string.IsNullOrWhiteSpace(request.Role) &&
                !roles.Any(r => string.Equals(r, request.Role, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var dto = _mapper.Map<UserDto>(u);
            dto.Roles = roles;
            dtos.Add(dto);
        }

        return new PaginatedList<UserDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
