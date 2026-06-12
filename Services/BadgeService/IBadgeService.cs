using SkillifyAPI.DTOs.Badge.BadgeDTO;

namespace SkillifyAPI.Services.BadgeService
{
    public interface IBadgeService
    {
        Task<IEnumerable<GetBadgeDTO>> GetAllAsync(CancellationToken ct = default);
    }
}
