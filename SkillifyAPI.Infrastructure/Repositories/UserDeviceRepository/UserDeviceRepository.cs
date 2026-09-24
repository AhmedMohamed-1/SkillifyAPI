using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.UserDeviceRepository
{
    public class UserDeviceRepository : IUserDeviceRepository
    {
        private readonly AppDbContext _context;

        public UserDeviceRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken ct = default)
            => _context.UserDevices.FirstOrDefaultAsync(d => d.FcmToken == fcmToken, ct);

        public async Task<IReadOnlyList<string>> GetActiveTokensByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .Select(d => d.FcmToken)
                .ToListAsync(ct);
        }

        public async Task AddAsync(UserDevice device, CancellationToken ct = default)
        {
            await _context.UserDevices.AddAsync(device, ct);
        }

        public async Task DeactivateTokenAsync(string fcmToken, CancellationToken ct = default)
        {
            var device = await GetByTokenAsync(fcmToken, ct);
            if (device == null)
                return;

            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
