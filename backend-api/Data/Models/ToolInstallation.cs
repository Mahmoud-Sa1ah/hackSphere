namespace PentestHub.API.Data.Models;

public class ToolInstallation
{
    public int InstallationId { get; set; }
    public int UserId { get; set; }
    public int ToolId { get; set; }
    public string InstallationPath { get; set; } = string.Empty; // Full path to the tool executable
    public DateTime InstalledAt { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public bool IsVerified { get; set; } // Whether the file still exists and is valid

    // Navigation properties
    public User? User { get; set; }
    public Tool? Tool { get; set; }
}



