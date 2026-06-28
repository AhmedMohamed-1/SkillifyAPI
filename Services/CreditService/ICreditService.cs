using SkillifyAPI.DTOs.Gifts;
using SkillifyAPI.Models;

namespace SkillifyAPI.Services.CreditService
{
    public interface ICreditService
    {
        Task<GiftCreditResponseDto> GiveCreditsAsync(
            GiveGiftCreditsDto dto,
            CancellationToken ct = default);

        Task GiveCreditsToUsersAsync(
             IEnumerable<User> users,
             int amount,
             CancellationToken ct = default);

        Task AddCreditsAsync(
            int userId,
            int amount,
            TransactionType type,
            int? sessionId = null,
            CancellationToken ct = default);

        Task DeductCreditsAsync(
            int userId,
            int amount,
            TransactionType type,
            int? sessionId = null,
            CancellationToken ct = default);
    }
}
