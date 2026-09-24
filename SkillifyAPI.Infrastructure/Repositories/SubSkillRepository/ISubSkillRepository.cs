using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.SubSkillRepository
{
    public interface ISubSkillRepository
    {
        Task<IEnumerable<SubSkill>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<SubSkill>> GetByMainSkillIdAsync(int mainSkillId, CancellationToken ct = default);
        Task<SubSkill?> GetByIdAsync(int id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
