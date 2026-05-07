using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityService _identity;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IIdentityService identity, IMapper mapper)
    {
        _identity = identity;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _identity.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _identity.GetRolesAsync(user);
        return dto;
    }
}
