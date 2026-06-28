using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.RatingRepository
{
    public interface IRatingRepository
    {
        Task AddAsync(Rating rating, CancellationToken ct = default);
        Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Rating?> GetBySessionIdAsync(int sessionId, CancellationToken ct = default);
        Task<bool> ExistsForSessionAsync(int sessionId, CancellationToken ct = default);
        Task<IEnumerable<Rating>> GetReceivedByUserIdAsync(int userId, CancellationToken ct = default);
        Task<IEnumerable<Rating>> GetGivenByUserIdAsync(int userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
