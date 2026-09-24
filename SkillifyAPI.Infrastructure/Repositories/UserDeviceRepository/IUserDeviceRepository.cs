using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.UserDeviceRepository
{
    public interface IUserDeviceRepository
    {
        Task<UserDevice?> GetByTokenAsync(string fcmToken, CancellationToken ct = default);

        Task<IReadOnlyList<string>> GetActiveTokensByUserIdAsync(int userId, CancellationToken ct = default);

        Task AddAsync(UserDevice device, CancellationToken ct = default);

        Task DeactivateTokenAsync(string fcmToken, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
