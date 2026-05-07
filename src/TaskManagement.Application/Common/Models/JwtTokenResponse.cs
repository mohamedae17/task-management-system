namespace TaskManagement.Application.Common.Models;

public sealed class JwtTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
}
