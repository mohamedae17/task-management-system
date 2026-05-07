using MediatR;
using TaskManagement.Application.Features.Dashboard.Dtos;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetTasksByStatus;

public sealed record GetTasksByStatusQuery : IRequest<IReadOnlyList<TaskCountByStatusDto>>;
