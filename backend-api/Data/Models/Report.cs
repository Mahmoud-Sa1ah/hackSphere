namespace PentestHub.API.Data.Models;

public class Report
{
    public int ReportId { get; set; }
    public int UserId { get; set; }
    public int? ScanId { get; set; }
    public string? PdfPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ScanHistory? ScanHistory { get; set; }
}

