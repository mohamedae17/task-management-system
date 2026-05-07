using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Dtos;

public sealed class TaskCountByStatusDto
{
    public TaskItemStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public int Count { get; init; }
}
