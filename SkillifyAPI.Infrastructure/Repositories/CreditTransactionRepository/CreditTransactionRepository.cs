using Microsoft.EntityFrameworkCore;
using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.CreditTransactionRepository
{
    public class CreditTransactionRepository : ICreditTransactionRepository
    {
        private readonly AppDbContext _context;

        public CreditTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<CreditTransaction> Transactions, int CurrentBalance)> GetUserTransactionsAsync(int userId)
        {
            var transactions = await _context.CreditTransactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var currentBalance = await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => x.CreditBalance)
                .FirstOrDefaultAsync();

            return (transactions, currentBalance);
        }
    }
}
