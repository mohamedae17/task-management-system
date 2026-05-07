using MediatR;
using TaskManagement.Application.Features.Dashboard.Dtos;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetDashboardStatistics;

public sealed record GetDashboardStatisticsQuery : IRequest<DashboardStatisticsDto>;
