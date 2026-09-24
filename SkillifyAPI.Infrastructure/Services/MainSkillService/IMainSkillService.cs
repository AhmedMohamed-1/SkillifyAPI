using SkillifyAPI.DTOs.Skill.SkillDTO;

namespace SkillifyAPI.Services.MainSkillService
{
    public interface IMainSkillService
    {
        Task<IEnumerable<GetMainSkillDTO>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<GetMainSkillWithSubSkillsDTO>> GetAllWithSubSkillsAsync(CancellationToken ct = default);
        Task<GetMainSkillWithSubSkillsDTO?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GetMainSkillWithSubSkillsDTO?> GetBySlugAsync(string slug, CancellationToken ct = default);
    }
}
