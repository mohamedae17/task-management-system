using MediatR;
using TaskManagement.Application.Features.Comments.Dtos;

namespace TaskManagement.Application.Features.Comments.Commands.UpdateComment;

public sealed record UpdateCommentCommand(Guid CommentId, string Content) : IRequest<CommentDto>;
