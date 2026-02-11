namespace PentestHub.API.Data.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public string? Bio { get; set; }
    public bool RequiresTwoFactor { get; set; } // If true, client needs to prompt for 2FA code
    public bool IsTwoFactorEnabled { get; set; }
}

