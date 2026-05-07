using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Notifications.Dtos;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? ActionUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}
