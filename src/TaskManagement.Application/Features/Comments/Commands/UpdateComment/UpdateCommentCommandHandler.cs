using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Comments.Dtos;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Comments.Commands.UpdateComment;

public sealed class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var comment = await _db.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Comment), request.CommentId);

        if (comment.AuthorId != userId && !_currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenAccessException("Only the comment author or an admin can edit this comment.");

        comment.Content = request.Content;
        comment.IsEdited = true;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CommentDto>(comment);
    }
}
