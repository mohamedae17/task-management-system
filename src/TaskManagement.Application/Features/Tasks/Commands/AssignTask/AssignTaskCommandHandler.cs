using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.AssignTask;

public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;

    public AssignTaskCommandHandler(
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

    public async Task<Unit> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var isPrivileged = _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Manager);
        if (!isPrivileged && task.CreatorId != userId)
            throw new ForbiddenAccessException("Only the task creator or a manager can change the assignee.");

        if (request.AssigneeId is { } aid)
        {
            var exists = await _db.Users.AnyAsync(u => u.Id == aid && u.IsActive, cancellationToken);
            if (!exists) throw new BusinessRuleException("Assignee does not exist or is inactive.");
        }

        if (task.AssigneeId == request.AssigneeId)
            return Unit.Value;

        var previousAssignee = task.AssigneeId;
        task.AssigneeId = request.AssigneeId;
        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            request.AssigneeId is null ? ActivityAction.Unassigned : ActivityAction.Assigned,
            nameof(TaskItem), task.Id, taskId: task.Id,
            metadata: new { Previous = previousAssignee, Current = request.AssigneeId },
            cancellationToken: cancellationToken);

        if (request.AssigneeId is { } newId && newId != userId)
        {
            var notification = new Notification
            {
                RecipientId = newId,
                Type = NotificationType.TaskAssigned,
                Title = "Task assigned",
                Message = $"You were assigned task '{task.Title}'.",
                RelatedEntityType = nameof(TaskItem),
                RelatedEntityId = task.Id,
                ActionUrl = $"/tasks/{task.Id}"
            };
            await _db.Notifications.AddAsync(notification, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyUserAsync(newId, notification, cancellationToken);
        }

        await _notifications.NotifyTaskChangedAsync(task.Id, "assigned", new { task.Id, task.AssigneeId }, cancellationToken);
        return Unit.Value;
    }
}
