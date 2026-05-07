using MediatR;

namespace TaskManagement.Application.Features.Comments.Commands.DeleteComment;

public sealed record DeleteCommentCommand(Guid CommentId) : IRequest<Unit>;
