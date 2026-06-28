using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.SessionRepository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _context;

        public SessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Session session, CancellationToken ct = default)
        {
            await _context.Sessions.AddAsync(session, ct);
        }

        public async Task<Session?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Sessions.FindAsync(id , ct);
        }

        public async Task<Session?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _context.Sessions
                .Include(s => s.Requester)
                .Include(s => s.Helper)
                .Include(s => s.MainSkills)
                .Include(s => s.EscrowHold)
                .Include(s => s.SessionEvents)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<IEnumerable<Session>> GetRequestedSessionsAsync(int requesterId, CancellationToken ct = default)
        {
            return await _context.Sessions
                .AsNoTracking()
                .Include(s => s.Helper)
                .Include(s => s.MainSkills)
                .Where(s => s.RequesterId == requesterId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Session>> GetReceivedSessionsAsync(int helperId, CancellationToken ct = default)
        {
            return await _context.Sessions
                .AsNoTracking()
                .Include(s => s.Requester)
                .Include(s => s.MainSkills)
                .Where(s => s.HelperId == helperId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddEventAsync(SessionEvent sessionEvent, CancellationToken ct = default)
        {
            await _context.SessionEvents.AddAsync(sessionEvent, ct);
        }

        public async Task AddEscrowHoldAsync(EscrowHold escrowHold, CancellationToken ct = default)
        {
            await _context.EscrowHolds.AddAsync(escrowHold, ct);
        }


        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
