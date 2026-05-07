using MediatR;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQuery : PaginationRequest, IRequest<PaginatedList<TaskListItemDto>>
{
    public Guid? AssigneeId { get; set; }
    public Guid? CreatorId { get; set; }
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? LabelId { get; set; }
    public DateTime? DueBefore { get; set; }
    public DateTime? DueAfter { get; set; }
    public bool? IsOverdue { get; set; }
}
