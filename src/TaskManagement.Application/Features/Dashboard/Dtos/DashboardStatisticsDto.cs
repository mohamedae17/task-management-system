namespace TaskManagement.Application.Features.Dashboard.Dtos;

public sealed class DashboardStatisticsDto
{
    public int TotalTasks { get; init; }
    public int OpenTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int OverdueTasks { get; init; }
    public int DueThisWeek { get; init; }
    public int MyAssignedTasks { get; init; }
    public int MyOpenTasks { get; init; }
    public int UnreadNotifications { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
}
