using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.NotificationRepository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Notification notification,
            CancellationToken ct = default)
        {
            await _context.Notifications.AddAsync(notification, ct);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
        }

        public async Task<Notification?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id, ct);
        }

        public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(ct);

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
