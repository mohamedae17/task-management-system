using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Dashboard.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Dashboard.Queries.GetTasksByStatus;

public sealed class GetTasksByStatusQueryHandler
    : IRequestHandler<GetTasksByStatusQuery, IReadOnlyList<TaskCountByStatusDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTasksByStatusQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskCountByStatusDto>> Handle(
        GetTasksByStatusQuery request, CancellationToken cancellationToken)
    {
        var counts = await _db.Tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Always emit a row per known status — even zeros — so charts have a stable shape.
        return Enum.GetValues<TaskItemStatus>()
            .Select(s => new TaskCountByStatusDto
            {
                Status = s,
                StatusName = s.ToString(),
                Count = counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0
            })
            .ToList();
    }
}
