using SkillifyAPI.DTOs.Gifts;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.CreditRepository;
using SkillifyAPI.Repositories.UserRepository;
using SkillifyAPI.Services.NotificationService;

namespace SkillifyAPI.Services.CreditService
{
    public class CreditService : ICreditService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICreditRepository _creditRepository;
        private readonly INotificationService _notificationService;

        public CreditService(
            IUserRepository userRepository,
            ICreditRepository creditRepository,
            INotificationService notificationService)
        {
            _userRepository = userRepository;
            _creditRepository = creditRepository;
            _notificationService = notificationService;
        }

         public async Task<GiftCreditResponseDto> GiveCreditsAsync(
            GiveGiftCreditsDto dto,
            CancellationToken ct = default)
        {
            var user =
                await _userRepository.GetUserByIdAsync(dto.UserId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            user.CreditBalance += dto.Amount;

            var transaction = new CreditTransaction
            {
                UserId = user.Id,
                Type = TransactionType.GiftCredit,
                Amount = dto.Amount,
                BalanceAfter = user.CreditBalance,
                CreatedAt = DateTime.UtcNow
            };

            await _creditRepository.AddCreditTransactionAsync(
                transaction,
                ct);

            await _creditRepository.SaveChangesAsync(ct);

            await _notificationService.NotifyUserAsync(
                user.Id,
                "Gift Credits",
                $"You received {dto.Amount} credits.",
                ct);

            return new GiftCreditResponseDto
            {
                UserId = user.Id,
                GiftAmount = dto.Amount,
                NewBalance = user.CreditBalance,
                GrantedAt = transaction.CreatedAt
            };
        }

        public async Task GiveCreditsToUsersAsync(
             IEnumerable<User> users,int amount,
            CancellationToken ct = default)
        {
            foreach (var user in users)
            {
                if (user == null)
                    continue;

                user.CreditBalance += amount;
                user.LastGiftCreditAt = DateTime.UtcNow;

                await _creditRepository.AddCreditTransactionAsync(
                    new CreditTransaction
                    {
                        UserId = user.Id,
                        Type = TransactionType.GiftCredit,
                        Amount = amount,
                        BalanceAfter = user.CreditBalance,
                        CreatedAt = DateTime.UtcNow
                    },
                    ct);

                await _notificationService.NotifyUserAsync(
                    user.Id,
                    "Gift Credits",
                    $"You received {amount} credits.",
                    ct);
            }

            await _creditRepository.SaveChangesAsync(ct);
        }

        public async Task AddCreditsAsync(
            int userId,
            int amount,
            TransactionType type,
            int? sessionId = null,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            user.CreditBalance += amount;

            var transaction = new CreditTransaction
            {
                UserId = user.Id,
                Type = type,
                Amount = amount,
                BalanceAfter = user.CreditBalance,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            };

            await _creditRepository.AddCreditTransactionAsync(transaction, ct);
            await _creditRepository.SaveChangesAsync(ct);

            string title = "Credits Added";
            string message = $"You received {amount} credits.";

            switch (type)
            {
                case TransactionType.EscrowRelease:
                    title = "Credits Earned";
                    message = $"You earned {amount} credits from completing a session.";
                    break;
                case TransactionType.Refund:
                    title = "Credits Refunded";
                    message = $"You have been refunded {amount} credits.";
                    break;
                case TransactionType.GiftCredit:
                    title = "Gift Credits";
                    message = $"You received a gift of {amount} credits.";
                    break;
            }

            await _notificationService.NotifyUserAsync(userId, title, message, ct);
        }

        public async Task DeductCreditsAsync(
            int userId,
            int amount,
            TransactionType type,
            int? sessionId = null,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            if (user.CreditBalance < amount)
                throw new InvalidOperationException($"Insufficient credits. Required: {amount}, Available: {user.CreditBalance}.");

            user.CreditBalance -= amount;

            var transaction = new CreditTransaction
            {
                UserId = user.Id,
                Type = type,
                Amount = -amount,
                BalanceAfter = user.CreditBalance,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            };

            await _creditRepository.AddCreditTransactionAsync(transaction, ct);
            await _creditRepository.SaveChangesAsync(ct);
        }
    }
}
