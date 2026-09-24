using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.LanguageRepository
{
    public interface ILanguageRepository
    {
        Task<IEnumerable<Language>> GetAllAsync(CancellationToken ct = default);
        Task<Language?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
