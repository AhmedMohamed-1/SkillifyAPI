using SkillifyAPI.Data;
using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.CreditRepository
{
    public class CreditRepository : ICreditRepository
    {
        private readonly AppDbContext _context;

        public CreditRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddCreditTransactionAsync(
            CreditTransaction transaction,
            CancellationToken ct = default)
        {
            await _context.CreditTransactions
                .AddAsync(transaction, ct);
        }

        public async Task SaveChangesAsync(
            CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
