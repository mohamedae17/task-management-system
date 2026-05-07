using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Dtos;

public sealed class TaskCountByPriorityDto
{
    public TaskPriority Priority { get; init; }
    public string PriorityName { get; init; } = string.Empty;
    public int Count { get; init; }
}
