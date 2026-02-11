using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Hubs;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace PentestHub.API.Services;

public class LauncherService : ILauncherService
{
    private readonly IHubContext<LauncherHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly System.Collections.Generic.HashSet<string> ConnectedLaunchers = new();
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, System.Diagnostics.Process> RunningProcesses = new();

    public LauncherService(
        IHubContext<LauncherHub> hubContext,
        IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
    }

    public async Task LaunchToolAsync(int scanId, int userId, string toolName, string executablePath, string arguments)
    {
        _ = Task.Run(async () =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                try
                {
                    // Construct path to tool executable in tools-containerized directory
                    // Assuming structure: ../tools-containerized/{ToolName}/{BinaryName or ToolName.exe}
                    var currentDir = Directory.GetCurrentDirectory();
                    var toolsDir = Path.GetFullPath(Path.Combine(currentDir, "..", "tools-containerized"));
                    
                    // Use the specific folder for the tool
                    var toolFolder = Path.Combine(toolsDir, toolName);
                    
                    // Try to find the executable. Preference:
                    // 1. ToolName.exe inside ToolName folder
                    // 2. nmap.exe (common lowercase) inside ToolName folder
                    var executablePath = Path.Combine(toolFolder, $"{toolName}.exe");
                    
                    if (!File.Exists(executablePath))
                    {
                        // Try lowercase
                        executablePath = Path.Combine(toolFolder, $"{toolName.ToLower()}.exe");
                        
                        // If still not found, try without .exe (for linux compatibility or if extension is already in name, though user said .exe)
                        if (!File.Exists(executablePath))
                        {
                            executablePath = Path.Combine(toolFolder, toolName);
                        }
                    }

                    if (!File.Exists(executablePath))
                    {
                         await _hubContext.Clients.Group(scanId.ToString()).SendAsync("ScanOutput", new { output = $"[ERROR] Tool executable not found at: {executablePath}" });
                         return;
                    }

                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = toolFolder // Set working directory to the tool's folder
                    };

                    using var process = new System.Diagnostics.Process();
                    process.StartInfo = processStartInfo;
                    
                    RunningProcesses.TryAdd(scanId, process);
                    
                    var fullOutputBuilder = new System.Text.StringBuilder();

                    process.OutputDataReceived += async (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            var line = e.Data + Environment.NewLine;
                            lock(fullOutputBuilder) { fullOutputBuilder.Append(line); }
                            await _hubContext.Clients.All.SendAsync("ScanOutput", new { ScanId = scanId, Output = line });
                        }
                    };

                    process.ErrorDataReceived += async (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            var line = "ERROR: " + e.Data + Environment.NewLine;
                            lock(fullOutputBuilder) { fullOutputBuilder.Append(line); }
                            await _hubContext.Clients.All.SendAsync("ScanOutput", new { ScanId = scanId, Output = line });
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();
                    
                    RunningProcesses.TryRemove(scanId, out _);

                    // Process complete
                    await HandleScanCompleteAsync(scanId, userId, fullOutputBuilder.ToString());
                }
                catch (Exception ex)
                {
                    var errorMsg = $"FAILED TO LAUNCH TOOL: {ex.Message}";
                    await UpdateScanOutput(scanId, errorMsg);
                    await _hubContext.Clients.All.SendAsync("ScanOutput", new { ScanId = scanId, Output = errorMsg });
                }
            }
        });
        await Task.CompletedTask;
    }

    public async Task StopToolAsync(int scanId)
    {
        if (RunningProcesses.TryRemove(scanId, out var process))
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch (Exception ex)
            {
                // Process might have already exited or access denied
                Console.WriteLine($"Error stopping process for scan {scanId}: {ex.Message}");
            }

            // Create scope for DB update
            await UpdateScanOutput(scanId, $"{Environment.NewLine}[!] PROCESS STOPPED BY USER.{Environment.NewLine}");
            await _hubContext.Clients.All.SendAsync("ScanOutput", new { ScanId = scanId, Output = $"{Environment.NewLine}[!] PROCESS STOPPED BY USER.{Environment.NewLine}" });
        }
    }

    public async Task SendCommandAsync(int scanId, string toolName, string target, string arguments)
    {
        await Task.CompletedTask;
    }

    public Task<bool> IsLauncherConnectedAsync()
    {
        return Task.FromResult(ConnectedLaunchers.Count > 0);
    }

    public async Task UpdateScanOutput(int scanId, string output)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scan = await context.ScanHistory.FindAsync(scanId);
            if (scan != null)
            {
                scan.RawOutput = (scan.RawOutput ?? "") + output;
                await context.SaveChangesAsync();
            }
        }
    }

    public static void RegisterLauncher(string connectionId)
    {
        ConnectedLaunchers.Add(connectionId);
    }

    public static void UnregisterLauncher(string connectionId)
    {
        ConnectedLaunchers.Remove(connectionId);
    }

    public async Task HandleScanCompleteAsync(int scanId, int userId, string fullOutput)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var scan = await context.ScanHistory
                .Include(s => s.Tool)
                .FirstOrDefaultAsync(s => s.ScanId == scanId);

            if (scan != null)
            {
                scan.RawOutput = fullOutput;
                await context.SaveChangesAsync();

                // Generate AI analysis
                var aiAnalysis = await aiService.AnalyzeScanOutputAsync(new AIAnalyzeDto
                {
                    RawOutput = fullOutput,
                    Tool = scan.Tool?.ToolName ?? "Unknown",
                    Target = scan.Target
                });

                scan.AISummary = aiAnalysis.Summary;
                scan.AINextSteps = aiAnalysis.NextSteps;
                scan.VulnerabilityCount = aiAnalysis.VulnerabilityCount;
                await context.SaveChangesAsync();

                // Send notification
                await notificationService.CreateNotificationAsync(
                    userId,
                    $"Scan Complete: {scan.Tool?.ToolName}",
                    $"AI analysis ready for scan on {scan.Target}"
                );

                // Notify via SignalR
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ScanComplete", new
                {
                    ScanId = scanId,
                    Summary = aiAnalysis.Summary,
                    NextSteps = aiAnalysis.NextSteps
                });
            }
        }
    }
}

