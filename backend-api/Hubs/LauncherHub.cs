using Microsoft.AspNetCore.SignalR;
using PentestHub.API.Services;

namespace PentestHub.API.Hubs;

public class LauncherHub : Hub
{
    private readonly ILauncherService _launcherService;

    public LauncherHub(ILauncherService launcherService)
    {
        _launcherService = launcherService;
    }

    public override async Task OnConnectedAsync()
    {
        LauncherService.RegisterLauncher(Context.ConnectionId);
        await Clients.All.SendAsync("LauncherStatusChanged", new { IsOnline = true });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        LauncherService.UnregisterLauncher(Context.ConnectionId);
        await Clients.All.SendAsync("LauncherStatusChanged", new { IsOnline = false });
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendOutput(int scanId, string output)
    {
        await _launcherService.UpdateScanOutput(scanId, output);
        await Clients.All.SendAsync("ScanOutput", new { ScanId = scanId, Output = output });
    }

    public async Task ScanComplete(int scanId, int userId, string fullOutput)
    {
        if (_launcherService is LauncherService service)
        {
            await service.HandleScanCompleteAsync(scanId, userId, fullOutput);
        }
    }
}

