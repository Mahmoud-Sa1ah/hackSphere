namespace PentestHub.API.Data.DTOs;

public class AIAnalyzeDto
{
    public string RawOutput { get; set; } = string.Empty;
    public string Tool { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}

