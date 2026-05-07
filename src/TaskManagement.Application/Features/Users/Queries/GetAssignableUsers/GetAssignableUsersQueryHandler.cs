using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetAssignableUsers;

public sealed class GetAssignableUsersQueryHandler
    : IRequestHandler<GetAssignableUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private const int MaxResults = 50;

    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAssignableUsersQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle(
        GetAssignableUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking().Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.Email!, $"%{s}%") ||
                EF.Functions.Like(u.FirstName, $"%{s}%") ||
                EF.Functions.Like(u.LastName, $"%{s}%"));
        }

        return await query
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Take(MaxResults)
            .ProjectTo<UserSummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
