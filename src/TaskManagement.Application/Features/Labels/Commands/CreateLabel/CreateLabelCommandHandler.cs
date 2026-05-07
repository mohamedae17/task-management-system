using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Labels.Dtos;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Labels.Commands.CreateLabel;

public sealed class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, LabelDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateLabelCommandHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<LabelDto> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Labels.AnyAsync(l => l.Name == request.Name, cancellationToken))
            throw new ConflictException($"A label named '{request.Name}' already exists.");

        var label = new Label
        {
            Name = request.Name,
            ColorHex = request.ColorHex,
            Description = request.Description
        };

        await _db.Labels.AddAsync(label, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LabelDto>(label);
    }
}
