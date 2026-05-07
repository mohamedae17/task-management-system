using MediatR;
using TaskManagement.Application.Features.Labels.Dtos;

namespace TaskManagement.Application.Features.Labels.Queries.GetLabels;

public sealed record GetLabelsQuery : IRequest<IReadOnlyList<LabelDto>>;
