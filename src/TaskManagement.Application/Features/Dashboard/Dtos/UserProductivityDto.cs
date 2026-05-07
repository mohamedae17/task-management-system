namespace TaskManagement.Application.Features.Dashboard.Dtos;

public sealed class UserProductivityDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int AssignedTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int OverdueTasks { get; init; }
    public double CompletionRate => AssignedTasks == 0
        ? 0
        : Math.Round((double)CompletedTasks / AssignedTasks, 4);
}
