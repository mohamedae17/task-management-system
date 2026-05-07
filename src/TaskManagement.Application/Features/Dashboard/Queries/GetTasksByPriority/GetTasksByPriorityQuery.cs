using MediatR;
using TaskManagement.Application.Features.Dashboard.Dtos;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetTasksByPriority;

public sealed record GetTasksByPriorityQuery : IRequest<IReadOnlyList<TaskCountByPriorityDto>>;
