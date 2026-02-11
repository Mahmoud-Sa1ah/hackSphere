using System.ComponentModel.DataAnnotations;

namespace PentestHub.API.Data.Models;

public class UserBadge
{
    public int UserBadgeId { get; set; }
    public int UserId { get; set; }
    public int BadgeId { get; set; }
    public DateTime DateEarned { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Badge? Badge { get; set; }
}
