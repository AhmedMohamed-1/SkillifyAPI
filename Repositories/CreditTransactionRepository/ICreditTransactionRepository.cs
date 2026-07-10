using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.CreditTransactionRepository
{
    public interface ICreditTransactionRepository
    {
        Task<(List<CreditTransaction> Transactions, int CurrentBalance)> GetUserTransactionsAsync(int userId);
    }
}
