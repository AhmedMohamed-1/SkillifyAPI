using SkillifyAPI.DTOs.Skill.SkillDTO;

namespace SkillifyAPI.Services.SubSkillService
{
    public interface ISubSkillService
    {
        Task<IEnumerable<GetSubSkillDTO>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<GetSubSkillDTO>> GetByMainSkillIdAsync(int mainSkillId, CancellationToken ct = default);
        Task<GetSubSkillDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
