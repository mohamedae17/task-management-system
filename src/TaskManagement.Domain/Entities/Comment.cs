using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
}
