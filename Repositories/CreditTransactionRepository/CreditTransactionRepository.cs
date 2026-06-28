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

        public async Task<List<CreditTransaction>> GetUserTransactionsAsync(int userId)
        {
            return await _context.CreditTransactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
