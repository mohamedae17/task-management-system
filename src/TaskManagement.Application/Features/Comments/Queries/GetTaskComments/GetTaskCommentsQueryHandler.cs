using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Comments.Dtos;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Comments.Queries.GetTaskComments;

public sealed class GetTaskCommentsQueryHandler
    : IRequestHandler<GetTaskCommentsQuery, PaginatedList<CommentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetTaskCommentsQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CommentDto>> Handle(
        GetTaskCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Tasks.AnyAsync(t => t.Id == request.TaskId, cancellationToken))
            throw new NotFoundException(nameof(TaskItem), request.TaskId);

        var query = _db.Comments
            .AsNoTracking()
            .Where(c => c.TaskId == request.TaskId)
            .OrderBy(c => c.CreatedAt)
            .ProjectTo<CommentDto>(_mapper.ConfigurationProvider);

        return await PaginatedList<CommentDto>.CreateAsync(
            query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
