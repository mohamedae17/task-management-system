using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IActivityLogger activity,
        IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
        _activity = activity;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var task = await _db.Tasks
            .Include(t => t.TaskLabels)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), request.TaskId);

        EnsureUserCanModify(task, userId);

        var previousStatus = task.Status;
        var previousAssigneeId = task.AssigneeId;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssigneeId = request.AssigneeId;

        ApplyStatusTransition(task, request.Status);

        if (request.LabelIds is not null)
            await SyncLabelsAsync(task, request.LabelIds, userId, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            ActivityAction.Updated, nameof(TaskItem), task.Id, taskId: task.Id,
            cancellationToken: cancellationToken);

        // If the assignee changed and now points at someone other than the current user, notify.
        if (task.AssigneeId is { } newAssigneeId &&
            newAssigneeId != previousAssigneeId &&
            newAssigneeId != userId)
        {
            await CreateAndPushNotificationAsync(
                newAssigneeId,
                NotificationType.TaskAssigned,
                "Task reassigned to you",
                $"You were assigned task '{task.Title}'.",
                task.Id, cancellationToken);
        }

        if (previousStatus != TaskItemStatus.Completed && task.Status == TaskItemStatus.Completed)
        {
            // Notify the creator when their task is closed.
            if (task.CreatorId != userId)
            {
                await CreateAndPushNotificationAsync(
                    task.CreatorId,
                    NotificationType.TaskCompleted,
                    "Task completed",
                    $"Task '{task.Title}' was marked complete.",
                    task.Id, cancellationToken);
            }
        }

        await _notifications.NotifyTaskChangedAsync(task.Id, "updated", new { task.Id }, cancellationToken);

        var loaded = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        return _mapper.Map<TaskDto>(loaded);
    }

    private void EnsureUserCanModify(TaskItem task, Guid userId)
    {
        var isPrivileged = _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Manager);
        if (!isPrivileged && task.CreatorId != userId && task.AssigneeId != userId)
            throw new ForbiddenAccessException("Only the task creator, assignee, or a manager can modify this task.");
    }

    private static void ApplyStatusTransition(TaskItem task, TaskItemStatus newStatus)
    {
        if (task.Status == newStatus) return;

        task.Status = newStatus;
        if (newStatus == TaskItemStatus.InProgress && task.StartedAt is null)
            task.StartedAt = DateTime.UtcNow;
        if (newStatus == TaskItemStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;
        if (newStatus != TaskItemStatus.Completed)
            task.CompletedAt = null;
    }

    private async Task SyncLabelsAsync(
        TaskItem task, IReadOnlyList<Guid> labelIds, Guid userId, CancellationToken cancellationToken)
    {
        var existing = task.TaskLabels.ToDictionary(tl => tl.LabelId);
        var desired = labelIds.ToHashSet();

        foreach (var (id, link) in existing.Where(e => !desired.Contains(e.Key)).ToList())
            _db.TaskLabels.Remove(link);

        var toAdd = desired.Except(existing.Keys).ToList();
        if (toAdd.Count > 0)
        {
            var validIds = await _db.Labels
                .Where(l => toAdd.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            foreach (var lid in validIds)
                _db.TaskLabels.Add(new TaskLabel { TaskId = task.Id, LabelId = lid, AssignedBy = userId });
        }
    }

    private async Task CreateAndPushNotificationAsync(
        Guid recipientId, NotificationType type, string title, string message,
        Guid taskId, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            RecipientId = recipientId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityType = nameof(TaskItem),
            RelatedEntityId = taskId,
            ActionUrl = $"/tasks/{taskId}"
        };
        await _db.Notifications.AddAsync(notification, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await _notifications.NotifyUserAsync(recipientId, notification, cancellationToken);
    }
}
