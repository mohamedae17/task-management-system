using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Features.Dashboard.Dtos;
using TaskManagement.Application.Features.Dashboard.Queries.GetDashboardStatistics;
using TaskManagement.Application.Features.Dashboard.Queries.GetTasksByPriority;
using TaskManagement.Application.Features.Dashboard.Queries.GetTasksByStatus;
using TaskManagement.Application.Features.Dashboard.Queries.GetUserProductivity;

namespace TaskManagement.Api.Controllers;

[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ApiControllerBase
{
    [HttpGet("statistics")]
    public async Task<ActionResult<DashboardStatisticsDto>> Statistics(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetDashboardStatisticsQuery(), ct));

    [HttpGet("tasks-by-status")]
    public async Task<ActionResult<IReadOnlyList<TaskCountByStatusDto>>> TasksByStatus(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTasksByStatusQuery(), ct));

    [HttpGet("tasks-by-priority")]
    public async Task<ActionResult<IReadOnlyList<TaskCountByPriorityDto>>> TasksByPriority(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTasksByPriorityQuery(), ct));

    [HttpGet("productivity")]
    public async Task<ActionResult<IReadOnlyList<UserProductivityDto>>> Productivity(
        [FromQuery] int top = 10, CancellationToken ct = default) =>
        Ok(await Mediator.Send(new GetUserProductivityQuery(top), ct));
}
