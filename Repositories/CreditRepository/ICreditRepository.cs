using SkillifyAPI.Models;

namespace SkillifyAPI.Repositories.CreditRepository
{
    public interface ICreditRepository
    {
        Task AddCreditTransactionAsync(
            CreditTransaction transaction,
            CancellationToken ct = default);

        Task SaveChangesAsync(
            CancellationToken ct = default);
    }
}
