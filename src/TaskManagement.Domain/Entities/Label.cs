using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

public class Label : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#888888";
    public string? Description { get; set; }

    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}
