using FluentValidation;
using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.Helper;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.MainSkillRepository;
using SkillifyAPI.Repositories.SubSkillRepository;

namespace SkillifyAPI.Services.SubSkillService
{
    public class SubSkillService : ISubSkillService
    {
        private readonly ISubSkillRepository _repo;
        private readonly IMainSkillRepository _mainSkillRepo;
        private readonly ILogger<SubSkillService> _logger;


        public SubSkillService(
            ISubSkillRepository repo,
            IMainSkillRepository mainSkillRepo,
            ILogger<SubSkillService> _logger)
        {
            _repo = repo;
            _mainSkillRepo = mainSkillRepo;
            this._logger = _logger;
        }

        public async Task<IEnumerable<GetSubSkillDTO>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("subskill.get_all_requested {Service}", nameof(SubSkillService));
            var subSkills = await _repo.GetAllAsync(ct);
            return subSkills.Select(s => MapToDto(s));
        }

        public async Task<IEnumerable<GetSubSkillDTO>> GetByMainSkillIdAsync(int mainSkillId, CancellationToken ct = default)
        {
            _logger.LogInformation("subskill.get_by_mainskill_id_requested {Service} {MainSkillId}", nameof(SubSkillService), mainSkillId);
            if (!await _mainSkillRepo.ExistsAsync(mainSkillId, ct))
            {
                throw new KeyNotFoundException($"Main skill with ID {mainSkillId} not found.");
            }
            var subSkills = await _repo.GetByMainSkillIdAsync(mainSkillId, ct);
            return subSkills.Select(s => MapToDto(s));
        }

        public async Task<GetSubSkillDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            _logger.LogInformation("subskill.get_by_id_requested {Service} {Id}", nameof(SubSkillService), id);
            var subSkill = await _repo.GetByIdAsync(id, ct);
            return subSkill == null ? null : MapToDto(subSkill);
        }

        private static GetSubSkillDTO MapToDto(SubSkill s)
        {
            return new GetSubSkillDTO
            {
                Id = s.Id,
                Name = s.Name,
                IconKey = s.IconKey
            };
        }
    }
}
