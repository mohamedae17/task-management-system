using MediatR;
using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? AvatarUrl) : IRequest<UserDto>;
