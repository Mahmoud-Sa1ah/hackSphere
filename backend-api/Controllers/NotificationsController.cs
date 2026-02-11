using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(notifications);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        await _notificationService.MarkAsReadAsync(id, userId);
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }
}

