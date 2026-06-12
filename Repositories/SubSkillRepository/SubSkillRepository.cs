using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.SubSkillRepository
{
    public class SubSkillRepository : ISubSkillRepository
    {
        private readonly AppDbContext _context;

        public SubSkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubSkill>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.SubSkills.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);
        }

        public async Task<IEnumerable<SubSkill>> GetByMainSkillIdAsync(int mainSkillId, CancellationToken ct = default)
        {
            return await _context.SubSkills
                .AsNoTracking()
                .Where(s => s.MainSkillId == mainSkillId)
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
        }

        public Task<SubSkill?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.SubSkills.FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

    }
}
