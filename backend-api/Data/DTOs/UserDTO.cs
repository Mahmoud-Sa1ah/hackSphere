namespace PentestHub.API.Data.DTOs;

public class UserDTO
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public string? Bio { get; set; }
    public int Points { get; set; }
    public int CompletedRooms { get; set; }
    public int Streak { get; set; }
}
