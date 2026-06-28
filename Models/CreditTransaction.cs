namespace SkillifyAPI.Models
{
    public enum TransactionType
    {
        EscrowHold,
        EscrowRelease,
        CreditEarned,
        Refund,
        GiftCredit
    }

    public class CreditTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? SessionId { get; set; }
        public TransactionType Type { get; set; }
        public int Amount { get; set; }
        public string? Description { get; set; }
        public int BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Session? Session { get; set; }
    }
}
