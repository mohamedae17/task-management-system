using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

/// Pushes realtime notifications to a recipient. Implemented in the API layer
/// using SignalR; tests can stub it without taking a hub dependency.
public interface INotificationService
{
    Task NotifyUserAsync(Guid userId, Notification notification, CancellationToken cancellationToken = default);
    Task NotifyUsersAsync(IEnumerable<Guid> userIds, Notification notification, CancellationToken cancellationToken = default);
    Task BroadcastAsync(string eventName, object payload, CancellationToken cancellationToken = default);
    Task NotifyTaskChangedAsync(Guid taskId, string changeType, object payload, CancellationToken cancellationToken = default);
}
