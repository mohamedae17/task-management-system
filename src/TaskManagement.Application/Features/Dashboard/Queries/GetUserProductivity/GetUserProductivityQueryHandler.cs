using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Dashboard.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetUserProductivity;

public sealed class GetUserProductivityQueryHandler
    : IRequestHandler<GetUserProductivityQuery, IReadOnlyList<UserProductivityDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetUserProductivityQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<UserProductivityDto>> Handle(
        GetUserProductivityQuery request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var top = request.Top is < 1 or > 100 ? 10 : request.Top;

        var rows = await _db.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => new UserProductivityDto
            {
                UserId = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                AssignedTasks = _db.Tasks.Count(t => t.AssigneeId == u.Id),
                CompletedTasks = _db.Tasks.Count(t => t.AssigneeId == u.Id && t.Status == TaskItemStatus.Completed),
                OverdueTasks = _db.Tasks.Count(t =>
                    t.AssigneeId == u.Id &&
                    t.DueDate != null &&
                    t.DueDate < now &&
                    t.Status != TaskItemStatus.Completed)
            })
            .Where(p => p.AssignedTasks > 0)
            .OrderByDescending(p => p.CompletedTasks)
            .Take(top)
            .ToListAsync(cancellationToken);

        return rows;
    }
}
