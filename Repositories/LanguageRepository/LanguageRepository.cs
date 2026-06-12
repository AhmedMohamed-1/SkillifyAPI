using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.LanguageRepository
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AppDbContext _context;

        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Language>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Languages.AsNoTracking().OrderBy(l => l.Name).ToListAsync(ct);
        }

        public Task<Language?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Languages.FirstOrDefaultAsync(l => l.Id == id, ct);
        }

        public Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return _context.Languages.FirstOrDefaultAsync(l => l.Code == code, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return _context.Languages.AnyAsync(l => l.Id == id, ct);
        }
    }
}
