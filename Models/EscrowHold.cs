namespace SkillifyAPI.Models
{
    public enum EscrowStatus { Held, Released, Refunded }

    public class EscrowHold
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int RequesterId { get; set; }
        public int CreditsHeld { get; set; }
        public EscrowStatus Status { get; set; } = EscrowStatus.Held;
        public DateTime HeldAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        // Navigation
        public Session Session { get; set; } = null!;
        public User Requester { get; set; } = null!;
    }
}
