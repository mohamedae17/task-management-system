using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Dashboard.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetTasksByPriority;

public sealed class GetTasksByPriorityQueryHandler
    : IRequestHandler<GetTasksByPriorityQuery, IReadOnlyList<TaskCountByPriorityDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTasksByPriorityQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskCountByPriorityDto>> Handle(
        GetTasksByPriorityQuery request, CancellationToken cancellationToken)
    {
        var counts = await _db.Tasks
            .Where(t => t.Status != TaskItemStatus.Completed)
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<TaskPriority>()
            .Select(p => new TaskCountByPriorityDto
            {
                Priority = p,
                PriorityName = p.ToString(),
                Count = counts.FirstOrDefault(c => c.Priority == p)?.Count ?? 0
            })
            .ToList();
    }
}
