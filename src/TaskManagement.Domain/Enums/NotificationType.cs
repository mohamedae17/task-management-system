namespace TaskManagement.Domain.Enums;

public enum NotificationType
{
    TaskAssigned = 1,
    TaskUpdated = 2,
    TaskCompleted = 3,
    TaskDueSoon = 4,
    TaskOverdue = 5,
    CommentAdded = 6,
    Mentioned = 7,
    SystemAnnouncement = 8
}
