namespace TaskManagement.Application.Features.Labels.Dtos;

public sealed class LabelDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = "#888888";
    public string? Description { get; init; }
}
