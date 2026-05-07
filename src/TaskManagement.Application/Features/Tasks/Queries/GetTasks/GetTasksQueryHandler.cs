using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PaginatedList<TaskListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(IApplicationDbContext db, IDateTimeProvider clock, IMapper mapper)
    {
        _db = db;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TaskListItemDto>> Handle(
        GetTasksQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<TaskItem> query = _db.Tasks.AsNoTracking();

        if (request.AssigneeId is { } aid) query = query.Where(t => t.AssigneeId == aid);
        if (request.CreatorId is { } cid)  query = query.Where(t => t.CreatorId == cid);
        if (request.Status is { } status)  query = query.Where(t => t.Status == status);
        if (request.Priority is { } prio)  query = query.Where(t => t.Priority == prio);
        if (request.DueBefore is { } db)   query = query.Where(t => t.DueDate != null && t.DueDate <= db);
        if (request.DueAfter is { } da)    query = query.Where(t => t.DueDate != null && t.DueDate >= da);

        if (request.IsOverdue == true)
        {
            var now = _clock.UtcNow;
            query = query.Where(t =>
                t.DueDate != null &&
                t.DueDate < now &&
                t.Status != TaskItemStatus.Completed);
        }

        if (request.LabelId is { } lid)
            query = query.Where(t => t.TaskLabels.Any(tl => tl.LabelId == lid));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Title, $"%{s}%") ||
                (t.Description != null && EF.Functions.Like(t.Description, $"%{s}%")));
        }

        query = (request.SortBy?.ToLowerInvariant(), request.SortDescending) switch
        {
            ("title", true)     => query.OrderByDescending(t => t.Title),
            ("title", false)    => query.OrderBy(t => t.Title),
            ("duedate", true)   => query.OrderByDescending(t => t.DueDate),
            ("duedate", false)  => query.OrderBy(t => t.DueDate),
            ("priority", true)  => query.OrderByDescending(t => t.Priority),
            ("priority", false) => query.OrderBy(t => t.Priority),
            ("status", true)    => query.OrderByDescending(t => t.Status),
            ("status", false)   => query.OrderBy(t => t.Status),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            _                    => query.OrderByDescending(t => t.CreatedAt)
        };

        return await PaginatedList<TaskListItemDto>.CreateAsync(
            query.ProjectTo<TaskListItemDto>(_mapper.ConfigurationProvider),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
