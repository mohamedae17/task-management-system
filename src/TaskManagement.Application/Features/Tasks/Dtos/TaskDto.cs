using TaskManagement.Application.Features.Labels.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Dtos;

public sealed class TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }

    public Guid CreatorId { get; init; }
    public string CreatorName { get; init; } = string.Empty;

    public Guid? ParentTaskId { get; init; }

    public int CommentCount { get; init; }
    public int AttachmentCount { get; init; }

    public IReadOnlyList<LabelDto> Labels { get; init; } = Array.Empty<LabelDto>();

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
