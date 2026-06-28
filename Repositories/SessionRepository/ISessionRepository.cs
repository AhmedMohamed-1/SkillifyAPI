using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.SessionRepository
{
    public interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken ct = default);
        Task<Session?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Session>> GetRequestedSessionsAsync(int requesterId, CancellationToken ct = default);
        Task<IEnumerable<Session>> GetReceivedSessionsAsync(int helperId, CancellationToken ct = default);
        Task AddEventAsync(SessionEvent sessionEvent, CancellationToken ct = default);
        Task AddEscrowHoldAsync(EscrowHold escrowHold, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
