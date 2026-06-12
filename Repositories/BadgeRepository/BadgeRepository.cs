using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.BadgeRepository;

namespace SkillifyAPI.Repositories.BadgeRepository
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly AppDbContext _context;

        public BadgeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Badge>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Badges.AsNoTracking().ToListAsync(ct);
        }
    }
}
