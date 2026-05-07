using MediatR;
using TaskManagement.Application.Features.Labels.Dtos;

namespace TaskManagement.Application.Features.Labels.Commands.CreateLabel;

public sealed record CreateLabelCommand(
    string Name,
    string ColorHex,
    string? Description) : IRequest<LabelDto>;
