using SkillifyAPI.DTOs.CreditTransaction;

namespace SkillifyAPI.Services.CreditTransactionService
{
    public interface ICreditTransactionService
    {
        Task<CreditTransactionHistoryDto> GetUserHistoryAsync(int userId);
    }
}
