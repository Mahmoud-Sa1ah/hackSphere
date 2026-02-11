namespace PentestHub.API.Data.DTOs;

public class ScanRequestDto
{
    public int ToolId { get; set; }
    public string Target { get; set; } = string.Empty;
    public string? Arguments { get; set; }
}

