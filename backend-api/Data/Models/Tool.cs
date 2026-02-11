namespace PentestHub.API.Data.Models;

public class Tool
{
    public int ToolId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? BinaryName { get; set; } // Name of the executable file (e.g., "nmap.exe", "nmap")
    public string? RequiredExtensions { get; set; } // Comma-separated extensions (e.g., ".exe,.bat,.cmd")
    public string? DownloadUrl { get; set; } // URL or instructions for downloading the tool
    public byte[]? PackageData { get; set; } // Zip file content stored in database
    public string? PackageExtension { get; set; } // Extension of the uploaded package (e.g., ".zip", ".pdf")
}

