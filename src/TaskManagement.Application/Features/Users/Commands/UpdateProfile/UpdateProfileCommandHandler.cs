using AutoMapper;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Features.Users.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;
    private readonly IActivityLogger _activity;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUser,
        IIdentityService identity,
        IActivityLogger activity,
        IMapper mapper)
    {
        _currentUser = currentUser;
        _identity = identity;
        _activity = activity;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedException();

        var user = await _identity.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var result = await _identity.UpdateProfileAsync(user, request.FirstName, request.LastName, request.AvatarUrl);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join("; ", result.Errors));

        await _activity.LogAsync(ActivityAction.Updated, "User", user.Id, cancellationToken: cancellationToken);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _identity.GetRolesAsync(user);
        return dto;
    }
}
