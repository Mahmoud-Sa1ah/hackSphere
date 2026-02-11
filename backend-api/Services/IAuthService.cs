using PentestHub.API.Data.DTOs;

namespace PentestHub.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    
    // Password Reset
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    // 2FA
    Task<string> GenerateTwoFactorSecretAsync(int userId); // Returns QR Code URI
    Task<bool> EnableTwoFactorAsync(int userId, string code);
    Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(TwoFactorDto dto);
}

