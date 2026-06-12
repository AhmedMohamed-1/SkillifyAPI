using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.MainSkillRepository
{
    public interface IMainSkillRepository
    {
        Task<IEnumerable<MainSkill>> GetAllAsync(bool includeSubSkills = false, CancellationToken ct = default);
        Task<MainSkill?> GetByIdAsync(int id, bool includeSubSkills = false, CancellationToken ct = default);
        Task<MainSkill?> GetBySlugAsync(string slug, bool includeSubSkills = false, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
