using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Dashboard.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetDashboardStatistics;

public sealed class GetDashboardStatisticsQueryHandler
    : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatisticsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetDashboardStatisticsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DashboardStatisticsDto> Handle(
        GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var weekAhead = now.AddDays(7);
        var meId = _currentUser.UserId;

        var totalTasks = await _db.Tasks.CountAsync(cancellationToken);
        var completedTasks = await _db.Tasks
            .CountAsync(t => t.Status == TaskItemStatus.Completed, cancellationToken);
        var overdueTasks = await _db.Tasks
            .CountAsync(t => t.DueDate != null && t.DueDate < now && t.Status != TaskItemStatus.Completed, cancellationToken);
        var dueThisWeek = await _db.Tasks
            .CountAsync(t => t.DueDate != null && t.DueDate >= now && t.DueDate <= weekAhead && t.Status != TaskItemStatus.Completed, cancellationToken);

        var myAssigned = meId is { } id1
            ? await _db.Tasks.CountAsync(t => t.AssigneeId == id1, cancellationToken)
            : 0;
        var myOpen = meId is { } id2
            ? await _db.Tasks.CountAsync(t => t.AssigneeId == id2 && t.Status != TaskItemStatus.Completed, cancellationToken)
            : 0;
        var unread = meId is { } id3
            ? await _db.Notifications.CountAsync(n => n.RecipientId == id3 && !n.IsRead, cancellationToken)
            : 0;

        var totalUsers = await _db.Users.CountAsync(cancellationToken);
        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, cancellationToken);

        return new DashboardStatisticsDto
        {
            TotalTasks = totalTasks,
            OpenTasks = totalTasks - completedTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = overdueTasks,
            DueThisWeek = dueThisWeek,
            MyAssignedTasks = myAssigned,
            MyOpenTasks = myOpen,
            UnreadNotifications = unread,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers
        };
    }
}
