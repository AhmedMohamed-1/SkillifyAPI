using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.RatingRepository
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _context;

        public RatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Rating rating, CancellationToken ct = default)
        {
            await _context.Ratings.AddAsync(rating, ct);
        }

        public async Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Ratings
                .AsNoTracking()
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<Rating?> GetBySessionIdAsync(int sessionId, CancellationToken ct = default)
        {
            return await _context.Ratings
                .AsNoTracking()
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .FirstOrDefaultAsync(r => r.SessionId == sessionId, ct);
        }

        public async Task<bool> ExistsForSessionAsync(int sessionId, CancellationToken ct = default)
        {
            return await _context.Ratings
                .AnyAsync(r => r.SessionId == sessionId, ct);
        }

        public async Task<IEnumerable<Rating>> GetReceivedByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Ratings
                .AsNoTracking()
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Rating>> GetGivenByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Ratings
                .AsNoTracking()
                .Include(r => r.Reviewee)
                .Where(r => r.ReviewerId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
