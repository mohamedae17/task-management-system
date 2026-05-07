using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Features.Auth.Commands.ChangePassword;
using TaskManagement.Application.Features.Auth.Commands.ForgotPassword;
using TaskManagement.Application.Features.Auth.Commands.Login;
using TaskManagement.Application.Features.Auth.Commands.Logout;
using TaskManagement.Application.Features.Auth.Commands.RefreshToken;
using TaskManagement.Application.Features.Auth.Commands.Register;
using TaskManagement.Application.Features.Auth.Commands.ResetPassword;
using TaskManagement.Application.Features.Auth.Commands.VerifyEmail;
using TaskManagement.Application.Features.Auth.Dtos;

namespace TaskManagement.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);
    public sealed record LogoutRequest(string? RefreshToken, bool AllSessions = false);
    public sealed record ForgotPasswordRequest(string Email);
    public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);
    public sealed record VerifyEmailRequest(Guid UserId, string Token);
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegisterCommand(body.Email, body.Password, body.FirstName, body.LastName, IpAddress()), ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var result = await Mediator.Send(new LoginCommand(body.Email, body.Password, IpAddress()), ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequest body, CancellationToken ct)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(body.RefreshToken, IpAddress()), ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest body, CancellationToken ct)
    {
        await Mediator.Send(new LogoutCommand(body.RefreshToken, body.AllSessions, IpAddress()), ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ForgotPasswordCommand(body.Email), ct);
        // Always 204 — never reveal whether the email is registered.
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ResetPasswordCommand(body.UserId, body.Token, body.NewPassword), ct);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest body, CancellationToken ct)
    {
        await Mediator.Send(new VerifyEmailCommand(body.UserId, body.Token), ct);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ChangePasswordCommand(body.CurrentPassword, body.NewPassword), ct);
        return NoContent();
    }
}
