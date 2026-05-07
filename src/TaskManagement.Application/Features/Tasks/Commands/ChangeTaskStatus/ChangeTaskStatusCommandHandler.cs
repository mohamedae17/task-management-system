using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler : IRequestHandler<ChangeTaskStatusCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;

    public ChangeTaskStatusCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IActivityLogger activity)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
        _activity = activity;
    }

    public async Task<Unit> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var isPrivileged = _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Manager);
        if (!isPrivileged && task.AssigneeId != userId && task.CreatorId != userId)
            throw new ForbiddenAccessException("Only the task creator, assignee, or a manager can change status.");

        if (task.Status == request.Status)
            return Unit.Value;

        var previous = task.Status;
        task.Status = request.Status;

        if (request.Status == TaskItemStatus.InProgress && task.StartedAt is null)
            task.StartedAt = DateTime.UtcNow;
        if (request.Status == TaskItemStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;
        else
            task.CompletedAt = null;

        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            ActivityAction.StatusChanged, nameof(TaskItem), task.Id, taskId: task.Id,
            metadata: new { Previous = previous, Current = request.Status },
            cancellationToken: cancellationToken);

        if (request.Status == TaskItemStatus.Completed && task.CreatorId != userId)
        {
            var notification = new Notification
            {
                RecipientId = task.CreatorId,
                Type = NotificationType.TaskCompleted,
                Title = "Task completed",
                Message = $"Task '{task.Title}' was marked complete.",
                RelatedEntityType = nameof(TaskItem),
                RelatedEntityId = task.Id,
                ActionUrl = $"/tasks/{task.Id}"
            };
            await _db.Notifications.AddAsync(notification, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyUserAsync(task.CreatorId, notification, cancellationToken);
        }

        await _notifications.NotifyTaskChangedAsync(task.Id, "status-changed",
            new { task.Id, Status = task.Status }, cancellationToken);

        return Unit.Value;
    }
}
