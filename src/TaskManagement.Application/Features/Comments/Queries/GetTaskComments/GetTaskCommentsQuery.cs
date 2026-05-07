using MediatR;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Comments.Dtos;

namespace TaskManagement.Application.Features.Comments.Queries.GetTaskComments;

public sealed class GetTaskCommentsQuery : PaginationRequest, IRequest<PaginatedList<CommentDto>>
{
    public Guid TaskId { get; init; }

    public GetTaskCommentsQuery(Guid taskId)
    {
        TaskId = taskId;
        PageSize = 50;
    }
}
