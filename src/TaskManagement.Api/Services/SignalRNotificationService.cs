using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Hubs;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Api.Services;

public sealed class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRNotificationService(IHubContext<NotificationsHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyUserAsync(Guid userId, Notification notification, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(NotificationsHub.GroupForUser(userId.ToString()))
            .SendAsync("notification", Project(notification), cancellationToken);

    public Task NotifyUsersAsync(IEnumerable<Guid> userIds, Notification notification, CancellationToken cancellationToken = default)
    {
        var groups = userIds.Select(id => NotificationsHub.GroupForUser(id.ToString())).ToArray();
        return _hub.Clients.Groups(groups)
            .SendAsync("notification", Project(notification), cancellationToken);
    }

    public Task BroadcastAsync(string eventName, object payload, CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(eventName, payload, cancellationToken);

    public Task NotifyTaskChangedAsync(Guid taskId, string changeType, object payload, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(NotificationsHub.GroupForTask(taskId))
            .SendAsync("task-changed", new { taskId, changeType, payload }, cancellationToken);

    private static object Project(Notification n) => new
    {
        n.Id,
        Type = n.Type.ToString(),
        n.Title,
        n.Message,
        n.IsRead,
        n.RelatedEntityType,
        n.RelatedEntityId,
        n.ActionUrl,
        n.CreatedAt
    };
}
