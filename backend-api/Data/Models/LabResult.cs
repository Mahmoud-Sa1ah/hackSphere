namespace PentestHub.API.Data.Models;

public class LabResult
{
    public int ResultId { get; set; }
    public int LabId { get; set; }
    public int UserId { get; set; }
    public int? Score { get; set; }
    public DateTime? CompletionTime { get; set; }
    public string? Details { get; set; }
    public string? AIFeedback { get; set; }

    public Lab? Lab { get; set; }
    public User? User { get; set; }
}

