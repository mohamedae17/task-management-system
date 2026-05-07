using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Tasks.Dtos;

namespace TaskManagement.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == request.TaskId)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException("Task", request.TaskId);
    }
}
