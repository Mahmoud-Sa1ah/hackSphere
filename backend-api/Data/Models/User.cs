namespace PentestHub.API.Data.Models;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? TwoFactorSecret { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public string? ProfilePhoto { get; set; }
    public string? Bio { get; set; }
    
    // Gamification properties
    public int Points { get; set; }
    public int CompletedRooms { get; set; }
    public int Streak { get; set; }
    public DateTime? LastLabSolvedDate { get; set; }

    public Role? Role { get; set; }
}

