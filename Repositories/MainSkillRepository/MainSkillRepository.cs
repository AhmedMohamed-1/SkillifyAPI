using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.MainSkillRepository
{
    public class MainSkillRepository : IMainSkillRepository
    {
        private readonly AppDbContext _context;

        public MainSkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MainSkill>> GetAllAsync(bool includeSubSkills = false, CancellationToken ct = default)
        {
            var query = _context.MainSkills.AsNoTracking();
            if (includeSubSkills)
            {
                query = query.Include(m => m.SubSkills);
            }
            return await query.OrderBy(m => m.Name).ToListAsync(ct);
        }

        public Task<MainSkill?> GetByIdAsync(int id, bool includeSubSkills = false, CancellationToken ct = default)
        {
            var query = _context.MainSkills.AsQueryable();
            if (includeSubSkills)
            {
                query = query.Include(m => m.SubSkills);
            }
            return query.FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public Task<MainSkill?> GetBySlugAsync(string slug, bool includeSubSkills = false, CancellationToken ct = default)
        {
            var query = _context.MainSkills.AsQueryable();
            if (includeSubSkills)
            {
                query = query.Include(m => m.SubSkills);
            }
            return query.FirstOrDefaultAsync(m => m.Slug == slug, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return _context.MainSkills.AnyAsync(m => m.Id == id, ct);
        }
    }
}
