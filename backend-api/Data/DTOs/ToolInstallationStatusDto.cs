namespace PentestHub.API.Data.DTOs;

public class ToolInstallationStatusDto
{
    public bool IsInstalled { get; set; }
    public string? InstallationPath { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? InstalledAt { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class VerifyInstallationRequestDto
{
    public string InstallationPath { get; set; } = string.Empty;
}



