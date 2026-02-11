namespace PentestHub.API.Data.Models;

public class KnowledgeBase
{
    public int KnowledgeBaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // Comma-separated tags
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; } // Admin user ID who created it
}

