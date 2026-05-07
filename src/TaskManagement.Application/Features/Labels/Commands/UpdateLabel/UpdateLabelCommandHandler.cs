using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Labels.Dtos;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Labels.Commands.UpdateLabel;

public sealed class UpdateLabelCommandHandler : IRequestHandler<UpdateLabelCommand, LabelDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateLabelCommandHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<LabelDto> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await _db.Labels.FirstOrDefaultAsync(l => l.Id == request.LabelId, cancellationToken)
            ?? throw new NotFoundException(nameof(Label), request.LabelId);

        if (!string.Equals(label.Name, request.Name, StringComparison.OrdinalIgnoreCase) &&
            await _db.Labels.AnyAsync(l => l.Name == request.Name && l.Id != label.Id, cancellationToken))
        {
            throw new ConflictException($"A label named '{request.Name}' already exists.");
        }

        label.Name = request.Name;
        label.ColorHex = request.ColorHex;
        label.Description = request.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LabelDto>(label);
    }
}
