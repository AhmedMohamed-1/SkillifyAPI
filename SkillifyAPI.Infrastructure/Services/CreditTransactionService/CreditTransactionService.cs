using SkillifyAPI.DTOs.CreditTransaction;
using SkillifyAPI.Repositories.CreditTransactionRepository;

namespace SkillifyAPI.Services.CreditTransactionService
{
    public class CreditTransactionService : ICreditTransactionService
    {
        private readonly ICreditTransactionRepository _repo;

        public CreditTransactionService(ICreditTransactionRepository repo)
        {
            _repo = repo;
        }

        public async Task<CreditTransactionHistoryDto> GetUserHistoryAsync(int userId)
        {
            var (transactions, currentBalance) = await _repo.GetUserTransactionsAsync(userId);

            var history = transactions.Select(x => new CreditTransactionDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Amount = x.Amount,
                Type = x.Type.ToString(),
                Description = x.Description,
                CreatedAt = x.CreatedAt
            }).ToList();

            return new CreditTransactionHistoryDto
            {
                History = history,
                CurrentBalance = currentBalance
            };
        }
    }    
}

