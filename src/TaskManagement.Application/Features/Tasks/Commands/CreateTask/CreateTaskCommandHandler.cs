using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(
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

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } creatorId)
            throw new UnauthorizedException();

        if (request.AssigneeId is { } aid && !await _db.Users.AnyAsync(u => u.Id == aid, cancellationToken))
            throw new BusinessRuleException("Assignee does not exist.");

        if (request.ParentTaskId is { } pid && !await _db.Tasks.AnyAsync(t => t.Id == pid, cancellationToken))
            throw new BusinessRuleException("Parent task does not exist.");

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            CreatorId = creatorId,
            AssigneeId = request.AssigneeId,
            ParentTaskId = request.ParentTaskId,
            StartedAt = request.Status == TaskItemStatus.InProgress ? DateTime.UtcNow : null,
            CompletedAt = request.Status == TaskItemStatus.Completed ? DateTime.UtcNow : null
        };

        await _db.Tasks.AddAsync(task, cancellationToken);

        if (request.LabelIds is { Count: > 0 })
        {
            var validIds = await _db.Labels
                .Where(l => request.LabelIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            foreach (var lid in validIds)
            {
                _db.TaskLabels.Add(new TaskLabel
                {
                    TaskId = task.Id,
                    LabelId = lid,
                    AssignedBy = creatorId
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            ActivityAction.Created, nameof(TaskItem), task.Id, taskId: task.Id,
            cancellationToken: cancellationToken);

        if (task.AssigneeId is { } assigneeId && assigneeId != creatorId)
        {
            var notification = new Notification
            {
                RecipientId = assigneeId,
                Type = NotificationType.TaskAssigned,
                Title = "New task assigned",
                Message = $"You were assigned task '{task.Title}'.",
                RelatedEntityType = nameof(TaskItem),
                RelatedEntityId = task.Id,
                ActionUrl = $"/tasks/{task.Id}"
            };
            await _db.Notifications.AddAsync(notification, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyUserAsync(assigneeId, notification, cancellationToken);
        }

        await _notifications.NotifyTaskChangedAsync(task.Id, "created", new { task.Id, task.Title }, cancellationToken);

        var loaded = await _db.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        return _mapper.Map<TaskDto>(loaded);
    }
}
