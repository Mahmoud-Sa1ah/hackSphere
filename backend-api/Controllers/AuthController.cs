using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            return BadRequest(new { message = "Email already exists" });
        }
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
        if (!success)
        {
            return BadRequest(new { message = "Invalid old password" });
        }
        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        // Always return success to prevent email enumeration (or follow reqs. I'll just say Sent)
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired token" });
        }
        return Ok(new { message = "Password reset successfully" });
    }

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var uri = await _authService.GenerateTwoFactorSecretAsync(userId);
        return Ok(new { uri });
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor([FromBody] TwoFactorDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _authService.EnableTwoFactorAsync(userId, dto.Code);
        if (!success)
        {
            return BadRequest(new { message = "Invalid code" });
        }
        return Ok(new { message = "Two Factor Authentication enabled successfully" });
    }

    [HttpPost("2fa/verify-login")]
    public async Task<IActionResult> VerifyTwoFactorLogin([FromBody] TwoFactorDto dto)
    {
        var result = await _authService.VerifyTwoFactorLoginAsync(dto);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid code or user" });
        }
        return Ok(result);
    }
}

public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

