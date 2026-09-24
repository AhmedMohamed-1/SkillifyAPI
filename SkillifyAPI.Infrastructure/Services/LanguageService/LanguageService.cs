using FluentValidation;
using SkillifyAPI.DTOs.Language.LanguageDTO;
using SkillifyAPI.Helper;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.LanguageRepository;

namespace SkillifyAPI.Services.LanguageService
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _repo;
        private readonly ILogger<LanguageService> _logger;


        public LanguageService(
            ILanguageRepository repo,
            ILogger<LanguageService> _logger)
        {
            _repo = repo;
            this._logger = _logger;
        }

        public async Task<IEnumerable<GetLanguageDTO>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("language.get_all_requested {Service}", nameof(LanguageService));
            var languages = await _repo.GetAllAsync(ct);
            return languages.Select(l => MapToDto(l));
        }

        public async Task<GetLanguageDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            _logger.LogInformation("language.get_by_id_requested {Service} {Id}", nameof(LanguageService), id);
            var language = await _repo.GetByIdAsync(id, ct);
            return language == null ? null : MapToDto(language);
        }

        public async Task<GetLanguageDTO?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var normalizedCode = code.Trim().ToLowerInvariant();
            _logger.LogInformation("language.get_by_code_requested {Service} {Code}", nameof(LanguageService), normalizedCode);
            var language = await _repo.GetByCodeAsync(normalizedCode, ct);
            return language == null ? null : MapToDto(language);
        }

        private static GetLanguageDTO MapToDto(Language l)
        {
            return new GetLanguageDTO
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code
            };
        }
    }
}
