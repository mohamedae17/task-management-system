namespace TaskManagement.Application.Features.Comments.Dtos;

public sealed class CommentDto
{
    public Guid Id { get; init; }
    public Guid TaskId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsEdited { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
