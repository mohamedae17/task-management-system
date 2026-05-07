using MediatR;
using TaskManagement.Application.Features.Comments.Dtos;

namespace TaskManagement.Application.Features.Comments.Commands.CreateComment;

public sealed record CreateCommentCommand(Guid TaskId, string Content) : IRequest<CommentDto>;
