using TaskManagement.Application.Features.Users.Dtos;

namespace TaskManagement.Application.Features.Auth.Dtos;

public sealed class AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public UserDto User { get; init; } = null!;
}
