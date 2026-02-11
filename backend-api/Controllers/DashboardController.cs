using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILauncherService _launcherService;

    public DashboardController(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILauncherService launcherService)
    {
        _context = context;
        _notificationService = notificationService;
        _launcherService = launcherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] string? timeFilter = "7d")
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // Calculate date range based on filter
        var startDate = timeFilter switch
        {
            "24h" => DateTime.UtcNow.AddHours(-24),
            "30d" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.UtcNow.AddDays(-7) // Default to 7 days
        };

        // Recent scans for activity feed
        var recentScans = await _context.ScanHistory
            .Include(s => s.Tool)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new
            {
                s.ScanId,
                ToolName = s.Tool != null ? s.Tool.ToolName : "Unknown",
                s.Target,
                s.CreatedAt,
                HasAISummary = !string.IsNullOrEmpty(s.AISummary)
            })
            .ToListAsync();

        // Activity feed from recent scans
        var activityFeed = recentScans
            .Select(s => new
            {
                Type = "scan",
                Message = $"{s.ToolName} scan on {s.Target}",
                Time = GetTimeAgo(s.CreatedAt),
                CreatedAt = s.CreatedAt
            })
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToList();

        var unreadNotifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
        var unreadCount = unreadNotifications.Count;

        var launcherStatus = await _launcherService.IsLauncherConnectedAsync();

        // Real statistics
        var totalScans = await _context.ScanHistory.CountAsync(s => s.UserId == userId);
        var scansLastWeek = await _context.ScanHistory
            .CountAsync(s => s.UserId == userId && s.CreatedAt >= DateTime.UtcNow.AddDays(-7));
        var scansPreviousWeek = await _context.ScanHistory
            .CountAsync(s => s.UserId == userId && 
                s.CreatedAt >= DateTime.UtcNow.AddDays(-14) && 
                s.CreatedAt < DateTime.UtcNow.AddDays(-7));
        var scanChange = scansPreviousWeek > 0 
            ? (int)(((double)(scansLastWeek - scansPreviousWeek) / scansPreviousWeek) * 100)
            : (scansLastWeek > 0 ? 100 : 0);

        // Sum vulnerabilities found by AI
        var vulnerabilityCount = await _context.ScanHistory
            .Where(s => s.UserId == userId)
            .SumAsync(s => s.VulnerabilityCount);
            
        var vulnerabilitiesLastWeek = await _context.ScanHistory
            .Where(s => s.UserId == userId && s.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .SumAsync(s => s.VulnerabilityCount);
            
        var vulnerabilitiesPreviousWeek = await _context.ScanHistory
            .Where(s => s.UserId == userId && 
                s.CreatedAt >= DateTime.UtcNow.AddDays(-14) && 
                s.CreatedAt < DateTime.UtcNow.AddDays(-7))
            .SumAsync(s => s.VulnerabilityCount);
            
        var vulnerabilityChange = vulnerabilitiesPreviousWeek > 0
            ? (int)(((double)(vulnerabilitiesLastWeek - vulnerabilitiesPreviousWeek) / vulnerabilitiesPreviousWeek) * 100)
            : (vulnerabilitiesLastWeek > 0 ? 100 : 0);

        // Count unique tools used
        var toolsUsed = await _context.ScanHistory
            .Where(s => s.UserId == userId)
            .Select(s => s.ToolId)
            .Distinct()
            .CountAsync();
        var toolsUsedLastWeek = await _context.ScanHistory
            .Where(s => s.UserId == userId && s.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .Select(s => s.ToolId)
            .Distinct()
            .CountAsync();
        var toolsUsedPreviousWeek = await _context.ScanHistory
            .Where(s => s.UserId == userId && 
                s.CreatedAt >= DateTime.UtcNow.AddDays(-14) && 
                s.CreatedAt < DateTime.UtcNow.AddDays(-7))
            .Select(s => s.ToolId)
            .Distinct()
            .CountAsync();
        var toolsUsedChange = toolsUsedPreviousWeek > 0
            ? (int)(((double)(toolsUsedLastWeek - toolsUsedPreviousWeek) / toolsUsedPreviousWeek) * 100)
            : (toolsUsedLastWeek > 0 ? 100 : 0);

        // Count active users (users who have scans in the last 7 days)
        var activeUsers = await _context.ScanHistory
            .Where(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync();

        // Tool category usage statistics
        var toolCategoryUsage = await _context.ScanHistory
            .Include(s => s.Tool)
            .Where(s => s.UserId == userId && s.CreatedAt >= startDate)
            .GroupBy(s => s.Tool != null ? s.Tool.Category : "Unknown")
            .Select(g => new
            {
                Category = g.Key ?? "Unknown",
                Usage = g.Count(),
                Tools = g.Select(s => s.ToolId).Distinct().Count()
            })
            .OrderByDescending(g => g.Usage)
            .Take(4)
            .ToListAsync();

        // System health data (based on successful scans vs total scans)
        var systemHealthData = await _context.ScanHistory
            .Where(s => s.UserId == userId && s.CreatedAt >= startDate)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Count(),
                Successful = g.Count(s => !string.IsNullOrEmpty(s.RawOutput))
            })
            .OrderBy(g => g.Date)
            .ToListAsync();

        var healthDataPoints = systemHealthData.Select(h => new
        {
            Time = h.Date.ToString("MM/dd"),
            Value = h.Total > 0 ? (int)((double)h.Successful / h.Total * 100) : 0
        }).ToList();

        return Ok(new
        {
            RecentScans = recentScans,
            ActivityFeed = activityFeed,
            UnreadNotifications = unreadCount,
            LauncherStatus = new
            {
                IsOnline = launcherStatus,
                Status = launcherStatus ? "Online" : "Offline"
            },
            Statistics = new
            {
                TotalScans = totalScans,
                TotalLabs = await _context.LabResults.CountAsync(r => r.UserId == userId),
                ScansChange = scanChange,
                VulnerabilitiesFound = vulnerabilityCount,
                VulnerabilitiesChange = vulnerabilityChange,
                ToolsUsed = toolsUsed,
                ToolsUsedChange = toolsUsedChange,
                ActiveUsers = activeUsers
            },
            ToolCategoryUsage = toolCategoryUsage,
            SystemHealthData = healthDataPoints
        });
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
        return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
    }
}

