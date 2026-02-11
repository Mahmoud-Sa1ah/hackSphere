namespace PentestHub.API.Data.Models;

public class AIConversation
{
    public int MsgId { get; set; }
    public int UserId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string? ResponseText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

