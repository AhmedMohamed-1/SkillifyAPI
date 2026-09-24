using SkillifyAPI.DTOs.Notification;

namespace SkillifyAPI.Services.NotificationService
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId, CancellationToken ct = default);
        Task<UnreadCountDto> GetUnreadCountAsync(int userId, CancellationToken ct = default);
        Task MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default);
        Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);
        Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default);
        Task RegisterDeviceAsync(int userId, string fcmToken, CancellationToken ct = default);
        Task UnregisterDeviceAsync(int userId, string fcmToken, CancellationToken ct = default);
    }
}
