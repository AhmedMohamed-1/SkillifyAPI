using SkillifyAPI.DTOs.Badge.BadgeDTO;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.BadgeRepository;

namespace SkillifyAPI.Services.BadgeService
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _repo;
        private readonly ILogger<BadgeService> _logger;

        public BadgeService(
            IBadgeRepository repo,
            ILogger<BadgeService> _logger)
        {
            _repo = repo;
            this._logger = _logger;
        }

        public async Task<IEnumerable<GetBadgeDTO>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("badge.get_all_requested {Service}", nameof(BadgeService));
            var badges = await _repo.GetAllAsync(ct);
            return badges.Select(b => MapToDto(b));
        }

        private static GetBadgeDTO MapToDto(Badge b)
        {
            return new GetBadgeDTO
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Description = b.Description,
                IconKey = b.IconKey
            };
        }
    }
}
