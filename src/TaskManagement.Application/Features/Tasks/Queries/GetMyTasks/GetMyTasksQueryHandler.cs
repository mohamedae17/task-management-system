using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Dtos;

namespace TaskManagement.Application.Features.Tasks.Queries.GetMyTasks;

public sealed class GetMyTasksQueryHandler : IRequestHandler<GetMyTasksQuery, PaginatedList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetMyTasksQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TaskListItemDto>> Handle(GetMyTasksQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var query = _db.Tasks
            .AsNoTracking()
            .Where(t => t.AssigneeId == userId);

        if (request.Status is { } status)
            query = query.Where(t => t.Status == status);

        return await PaginatedList<TaskListItemDto>.CreateAsync(
            query.OrderByDescending(t => t.CreatedAt)
                 .ProjectTo<TaskListItemDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
