using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.NotificationRepository
{
    public interface INotificationRepository
    {
        Task AddAsync(
            Notification notification,
            CancellationToken ct = default);

        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken ct = default);

        Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default);

        Task<Notification?> GetByIdAsync(int id, CancellationToken ct = default);

        Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
