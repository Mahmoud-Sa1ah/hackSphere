using System.ComponentModel.DataAnnotations;

namespace PentestHub.API.Data.Models;

public class Badge
{
    public int BadgeId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int PointsRequired { get; set; }
    
    [MaxLength(500)]
    public string? IconUrl { get; set; }
}
