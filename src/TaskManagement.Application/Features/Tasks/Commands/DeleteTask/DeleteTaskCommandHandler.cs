using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;

    public DeleteTaskCommandHandler(
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

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var isPrivileged = _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Manager);
        if (!isPrivileged && task.CreatorId != userId)
            throw new ForbiddenAccessException("Only the task creator or a manager can delete this task.");

        // Soft-delete is applied by AuditingSaveChangesInterceptor.
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            ActivityAction.Deleted, nameof(TaskItem), task.Id, taskId: task.Id,
            cancellationToken: cancellationToken);

        await _notifications.NotifyTaskChangedAsync(task.Id, "deleted", new { task.Id }, cancellationToken);

        return Unit.Value;
    }
}
