using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUser, IIdentityService identity, IMapper mapper)
    {
        _currentUser = currentUser;
        _identity = identity;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var user = await _identity.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _identity.GetRolesAsync(user);
        return dto;
    }
}
