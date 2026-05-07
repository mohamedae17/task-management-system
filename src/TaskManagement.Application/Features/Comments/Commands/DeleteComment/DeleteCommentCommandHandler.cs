using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Comments.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCommentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Comment), request.CommentId);

        if (comment.AuthorId != userId && !_currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenAccessException("Only the comment author or an admin can delete this comment.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
