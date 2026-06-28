using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.CreditTransactionRepository
{
    public interface ICreditTransactionRepository
    {
        Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId);
    }
}
