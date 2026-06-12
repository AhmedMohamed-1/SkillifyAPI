using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SignUpAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            return true;
        }

        public Task<User?> GetUserByIdAsync(int userId, CancellationToken ct = default)
            => _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

        public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
            => _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetUserForProfileAsync(int userId, CancellationToken ct = default)
            => _context.Users
                .AsNoTracking()
                .Include(u => u.Badges).ThenInclude(ub => ub.Badge)
                .Include(u => u.Languages).ThenInclude(ul => ul.Language)
                .Include(u => u.Skills).ThenInclude(s => s.Category)
                .Include(u => u.Skills).ThenInclude(s => s.SubSkills).ThenInclude(ss => ss.SubSkill)
                .Include(u => u.ReceivedRatings).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Users.AsNoTracking();
            var totalCount = await query.CountAsync(ct);
            var users = await query
                .Include(u => u.Skills).ThenInclude(s => s.Category)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            return (users, totalCount);
        }

        public Task<int> GetCompletedSessionsCountAsync(int userId, CancellationToken ct = default)
            => _context.Sessions.CountAsync(s =>
                s.Status == SessionStatus.Completed &&
                (s.RequesterId == userId || s.HelperId == userId), ct);

        public Task<bool> MainSkillExistsAsync(int mainSkillId, CancellationToken ct = default)
            => _context.MainSkills.AsNoTracking().AnyAsync(m => m.Id == mainSkillId, ct);

        public async Task<bool> SubSkillsExistForMainSkillAsync(int mainSkillId, IEnumerable<int> subSkillIds, CancellationToken ct = default)
        {
            var ids = subSkillIds.Distinct().ToArray();
            if (ids.Length == 0)
                return false;

            var count = await _context.SubSkills
                .AsNoTracking()
                .CountAsync(s => s.MainSkillId == mainSkillId && ids.Contains(s.Id), ct);

            return count == ids.Length;
        }

        public async Task RemoveUserSkillsAsync(int userId, CancellationToken ct = default)
        {
            var skills = await _context.UserSkills
                .Where(s => s.UserId == userId)
                .Include(s => s.SubSkills)
                .ToListAsync(ct);

            if (skills.Count == 0)
                return;

            _context.UserSkillSubSkills.RemoveRange(skills.SelectMany(s => s.SubSkills));
            _context.UserSkills.RemoveRange(skills);
        }

        public async Task AddUserSkillAsync(UserSkill userSkill, CancellationToken ct = default)
            => await _context.UserSkills.AddAsync(userSkill, ct);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
            => await _context.RefreshTokens.AddAsync(token, ct);

        public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
            => _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);

        private IQueryable<RefreshToken> GetActiveRefreshTokensQuery(int userId)
        {
            return _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensForUserAsync(int userId, CancellationToken ct = default)
        {
            return await GetActiveRefreshTokensQuery(userId).ToListAsync(ct);
        }

        public async Task RevokeAllForUserAsync(int userId, string reason, CancellationToken ct = default)
        {
            var tokens = await GetActiveRefreshTokensQuery(userId).ToListAsync(ct);
            if (!tokens.Any())
                return;

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokeReason = reason;
            }
        }

        public void UpdateRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
        }
    }
}
