using MediatR;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Queries.GetMyTasks;

public sealed class GetMyTasksQuery : PaginationRequest, IRequest<PaginatedList<TaskListItemDto>>
{
    public TaskItemStatus? Status { get; set; }
}
