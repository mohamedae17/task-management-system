using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Dtos;

public sealed class TaskListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public TaskItemStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public DateTime CreatedAt { get; init; }
}
