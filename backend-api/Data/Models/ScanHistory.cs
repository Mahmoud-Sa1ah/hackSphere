namespace PentestHub.API.Data.Models;

public class ScanHistory
{
    public int ScanId { get; set; }
    public int UserId { get; set; }
    public int ToolId { get; set; }
    public string Target { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? RawOutput { get; set; }
    public string? AISummary { get; set; }
    public string? AINextSteps { get; set; }
    public int VulnerabilityCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Tool? Tool { get; set; }
}

