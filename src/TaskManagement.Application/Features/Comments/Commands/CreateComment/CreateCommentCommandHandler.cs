using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Comments.Dtos;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Comments.Commands.CreateComment;

public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(
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

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var comment = new Comment
        {
            TaskId = request.TaskId,
            AuthorId = userId,
            Content = request.Content
        };

        await _db.Comments.AddAsync(comment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(
            ActivityAction.Commented, nameof(Comment), comment.Id, taskId: task.Id,
            cancellationToken: cancellationToken);

        // Notify creator + assignee (if not the commenter).
        var recipients = new HashSet<Guid> { task.CreatorId };
        if (task.AssigneeId is { } aid) recipients.Add(aid);
        recipients.Remove(userId);

        foreach (var recipientId in recipients)
        {
            var notification = new Notification
            {
                RecipientId = recipientId,
                Type = NotificationType.CommentAdded,
                Title = "New comment",
                Message = $"A new comment was added to task '{task.Title}'.",
                RelatedEntityType = nameof(TaskItem),
                RelatedEntityId = task.Id,
                ActionUrl = $"/tasks/{task.Id}"
            };
            await _db.Notifications.AddAsync(notification, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyUserAsync(recipientId, notification, cancellationToken);
        }

        await _notifications.NotifyTaskChangedAsync(task.Id, "comment-added",
            new { CommentId = comment.Id, comment.AuthorId }, cancellationToken);

        var loaded = await _db.Comments
            .AsNoTracking()
            .Include(c => c.Author)
            .FirstAsync(c => c.Id == comment.Id, cancellationToken);

        return _mapper.Map<CommentDto>(loaded);
    }
}
