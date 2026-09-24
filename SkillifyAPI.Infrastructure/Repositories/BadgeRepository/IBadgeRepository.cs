using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.BadgeRepository
{
    public interface IBadgeRepository
    {
        Task<IEnumerable<Badge>> GetAllAsync(CancellationToken ct = default);
    }
}
