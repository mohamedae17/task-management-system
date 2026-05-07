namespace TaskManagement.Domain.Entities;

public class TaskLabel
{
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public Guid LabelId { get; set; }
    public Label Label { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }
}
