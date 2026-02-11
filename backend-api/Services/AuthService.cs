using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Data.Models;
using PentestHub.API.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OtpNet;

namespace PentestHub.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        if (user.IsTwoFactorEnabled)
        {
            return new AuthResponseDto
            {
                UserId = user.UserId,
                RequiresTwoFactor = true
            };
        }

        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Learner",
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            ProfilePhoto = user.ProfilePhoto,
            Bio = user.Bio
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _userRepository.GetByEmailOrUsernameAsync(registerDto.Email) != null || 
            await _userRepository.GetByUsernameAsync(registerDto.Name) != null)
        {
            return null;
        }

        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            RoleId = registerDto.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Refetch to get Role
        var savedUser = await _userRepository.GetByIdAsync(user.UserId);
        if (savedUser == null) return null;

        var token = GenerateJwtToken(savedUser);

        return new AuthResponseDto
        {
            Token = token,
            UserId = savedUser.UserId,
            Name = savedUser.Name,
            Email = savedUser.Email,
            Role = savedUser.Role?.RoleName ?? "Learner",
            IsTwoFactorEnabled = savedUser.IsTwoFactorEnabled,
            ProfilePhoto = savedUser.ProfilePhoto,
            Bio = savedUser.Bio
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var resetLink = $"http://localhost:5173/reset-password?token={token}";
        await _emailService.SendEmailAsync(user.Email, "Reset Password", $"Click here to reset your password: <a href=\"{resetLink}\">{resetLink}</a>");
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // This is a bit specific, might need a custom query in repository if we want to be strict.
        // For now, I'll filter in memory after getting all users or add a method to Repo.
        // Actually, let's add a method to Repo for ResetToken.
        var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpires = null;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateTwoFactorSecretAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var key = KeyGeneration.GenerateRandomKey(20);
        var secret = Base32Encoding.ToString(key);
        
        user.TwoFactorSecret = secret;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return $"otpauth://totp/PentestHub:{user.Email}?secret={secret}&issuer=PentestHub";
    }

    public async Task<bool> EnableTwoFactorAsync(int userId, string code)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        var cleanCode = code.Replace(" ", "").Trim();
        var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
        var totp = new Totp(secretBytes);
        
        if (totp.VerifyTotp(cleanCode, out _, new VerificationWindow(2, 2)))
        {
            user.IsTwoFactorEnabled = true;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<AuthResponseDto?> VerifyTwoFactorLoginAsync(TwoFactorDto dto)
    {
        if (!dto.UserId.HasValue) return null;

        var user = await _userRepository.GetByIdAsync(dto.UserId.Value);
        if (user == null || !user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return null;
        }

        var cleanCode = dto.Code.Replace(" ", "").Trim();
        var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
        var totp = new Totp(secretBytes);

        if (totp.VerifyTotp(cleanCode, out _, new VerificationWindow(2, 2)))
        {
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "Learner",
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                ProfilePhoto = user.ProfilePhoto,
                Bio = user.Bio
            };
        }

        return null;
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32Characters");
        var issuer = _configuration["Jwt:Issuer"] ?? "PentestHub";
        var audience = _configuration["Jwt:Audience"] ?? "PentestHubUsers";
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Learner")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Profile management methods moved to UserService
}


