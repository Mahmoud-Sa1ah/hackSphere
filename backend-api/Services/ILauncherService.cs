namespace PentestHub.API.Services;

public interface ILauncherService
{
    Task SendCommandAsync(int scanId, string toolName, string target, string arguments);
    Task LaunchToolAsync(int scanId, int userId, string toolName, string executablePath, string arguments);
    Task StopToolAsync(int scanId);
    Task<bool> IsLauncherConnectedAsync();
    Task UpdateScanOutput(int scanId, string output);
}

