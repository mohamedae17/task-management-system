using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Labels.Dtos;

namespace TaskManagement.Application.Features.Labels.Queries.GetLabels;

public sealed class GetLabelsQueryHandler : IRequestHandler<GetLabelsQuery, IReadOnlyList<LabelDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetLabelsQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LabelDto>> Handle(GetLabelsQuery request, CancellationToken cancellationToken) =>
        await _db.Labels
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ProjectTo<LabelDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
