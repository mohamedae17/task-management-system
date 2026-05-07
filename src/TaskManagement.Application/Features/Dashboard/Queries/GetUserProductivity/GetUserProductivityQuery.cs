using MediatR;
using TaskManagement.Application.Features.Dashboard.Dtos;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetUserProductivity;

public sealed record GetUserProductivityQuery(int Top = 10)
    : IRequest<IReadOnlyList<UserProductivityDto>>;
