using TaskManagement.Domain.Common;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }

    public ActivityAction Action { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }

    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }

    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
