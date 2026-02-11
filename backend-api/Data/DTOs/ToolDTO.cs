namespace PentestHub.API.Data.DTOs;

public class ToolDTO
{
    public int ToolId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BinaryName { get; set; } = string.Empty;
    public string? RequiredExtensions { get; set; }
    public string? DownloadUrl { get; set; }
}
