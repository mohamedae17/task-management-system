using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

public class Attachment : BaseEntity
{
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public Guid UploadedById { get; set; }
    public AppUser UploadedBy { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
