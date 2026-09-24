using FluentValidation;
using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.Helper;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.MainSkillRepository;

namespace SkillifyAPI.Services.MainSkillService
{
    public class MainSkillService : IMainSkillService
    {
        private readonly IMainSkillRepository _repo;
        private readonly ILogger<MainSkillService> _logger;


        public MainSkillService(
            IMainSkillRepository repo,
            ILogger<MainSkillService> _logger)
        {
            _repo = repo;
            this._logger = _logger;
        }

        public async Task<IEnumerable<GetMainSkillDTO>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("mainskill.get_all_requested {Service}", nameof(MainSkillService));
            var skills = await _repo.GetAllAsync(false, ct);
            return skills.Select(m => new GetMainSkillDTO
            {
                Id = m.Id,
                Name = m.Name,
                Slug = m.Slug,
                IconKey = m.IconKey
            });
        }

        public async Task<IEnumerable<GetMainSkillWithSubSkillsDTO>> GetAllWithSubSkillsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("mainskill.get_all_with_subskills_requested {Service}", nameof(MainSkillService));
            var skills = await _repo.GetAllAsync(true, ct);
            return skills.Select(m => MapToWithSubSkillsDto(m));
        }

        public async Task<GetMainSkillWithSubSkillsDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            _logger.LogInformation("mainskill.get_by_id_requested {Service} {Id}", nameof(MainSkillService), id);
            var skill = await _repo.GetByIdAsync(id, true, ct);
            return skill == null ? null : MapToWithSubSkillsDto(skill);
        }

        public async Task<GetMainSkillWithSubSkillsDTO?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            _logger.LogInformation("mainskill.get_by_slug_requested {Service} {Slug}", nameof(MainSkillService), slug);
            var skill = await _repo.GetBySlugAsync(slug, true, ct);
            return skill == null ? null : MapToWithSubSkillsDto(skill);
        }

        private static GetMainSkillWithSubSkillsDTO MapToWithSubSkillsDto(MainSkill m)
        {
            return new GetMainSkillWithSubSkillsDTO
            {
                Id = m.Id,
                Name = m.Name,
                Slug = m.Slug,
                IconKey = m.IconKey,
                SubSkills = m.SubSkills.Select(s => new GetSubSkillDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    IconKey = s.IconKey
                }).ToList()
            };
        }
    }
}
