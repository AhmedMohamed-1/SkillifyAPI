using SkillifyAPI.DTOs.CreditTransaction;

namespace SkillifyAPI.Services.CreditTransactionService
{
    public interface ICreditTransactionService
    {
        Task<List<CreditTransactionDto>> GetUserHistoryAsync(int userId);
    }
}
