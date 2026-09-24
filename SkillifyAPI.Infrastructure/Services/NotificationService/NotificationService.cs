using SkillifyAPI.DTOs.Notification;
using SkillifyAPI.Firebase;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.NotificationRepository;
using SkillifyAPI.Repositories.UserDeviceRepository;

namespace SkillifyAPI.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IFirebaseNotificationService _firebaseNotificationService;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserDeviceRepository userDeviceRepository,
            IFirebaseNotificationService firebaseNotificationService)
        {
            _notificationRepository = notificationRepository;
            _userDeviceRepository = userDeviceRepository;
            _firebaseNotificationService = firebaseNotificationService;
        }

        public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId, CancellationToken ct = default)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, ct);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<UnreadCountDto> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            return new UnreadCountDto { Count = count };
        }

        public async Task MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, ct);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to access this notification.");

            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync(ct);
        }

        public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId, ct);
            await _notificationRepository.SaveChangesAsync(ct);
        }

        public async Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default)
        {
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _notificationRepository.SaveChangesAsync(ct);

            try
            {
                await _firebaseNotificationService.SendToUserAsync(userId, title, message, ct);
            }
            catch
            {
                // Push delivery failure should not roll back the stored notification.
            }
        }

        public async Task RegisterDeviceAsync(int userId, string fcmToken, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var existing = await _userDeviceRepository.GetByTokenAsync(fcmToken, ct);

            if (existing != null)
            {
                existing.UserId = userId;
                existing.IsActive = true;
                existing.UpdatedAt = now;
            }
            else
            {
                await _userDeviceRepository.AddAsync(new UserDevice
                {
                    UserId = userId,
                    FcmToken = fcmToken,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                }, ct);
            }

            await _userDeviceRepository.SaveChangesAsync(ct);
        }

        public async Task UnregisterDeviceAsync(int userId, string fcmToken, CancellationToken ct = default)
        {
            var device = await _userDeviceRepository.GetByTokenAsync(fcmToken, ct);

            if (device == null)
                return;

            if (device.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to unregister this device.");

            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;
            await _userDeviceRepository.SaveChangesAsync(ct);
        }
    }
}
