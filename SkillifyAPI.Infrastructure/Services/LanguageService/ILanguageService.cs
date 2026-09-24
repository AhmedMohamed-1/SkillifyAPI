using SkillifyAPI.DTOs.Language.LanguageDTO;

namespace SkillifyAPI.Services.LanguageService
{
    public interface ILanguageService
    {
        Task<IEnumerable<GetLanguageDTO>> GetAllAsync(CancellationToken ct = default);
        Task<GetLanguageDTO?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GetLanguageDTO?> GetByCodeAsync(string code, CancellationToken ct = default);
    }
}
