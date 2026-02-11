namespace PentestHub.API.Data.DTOs;

public class ChatDto
{
    public string Message { get; set; } = string.Empty;
    public string? FileData { get; set; } // Base64
    public string? FileName { get; set; }
    public string? FileType { get; set; }
}
