using PentestHub.API.Data.Models;

namespace PentestHub.API.Services;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(int userId, string title, string? message);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}

