using System.ComponentModel.DataAnnotations.Schema;

namespace SkillifyAPI.Models
{
    public enum SessionDuration { FifteenMin = 15, ThirtyMin = 30, SixtyMin = 60 }

    public enum SessionStatus
    {
        Pending,
        Accepted,
        Declined,
        ReOffered,
        Active,
        Completed,
        Cancelled,
        Expired
    }

    public class Session
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int HelperId { get; set; }
        public int MainSkillId { get; set; }
        public string Topic { get; set; } = null!;
        public string ProblemDescription { get; set; } = null!;
        public SessionDuration DurationMinutes { get; set; }
        public int CreditCost { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Pending;
        public DateTime ScheduledAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }



        // ZegoCloud video room fields
        public string? ZegoRoomId { get; set; }          // GUID string, set when Accepted
        public string? HangfireOpenJobId { get; set; }   // so you can cancel if needed
        public string? HangfireCloseJobId { get; set; }  // same



        // Navigation
        [ForeignKey(nameof(RequesterId))]
        [InverseProperty(nameof(User.RequestedSessions))]
        public User Requester { get; set; } = null!;

        [ForeignKey(nameof(HelperId))]
        [InverseProperty(nameof(User.HelpedSessions))]
        public User Helper { get; set; } = null!;

        public MainSkill MainSkills { get; set; } = null!;
        public ICollection<SessionEvent> SessionEvents { get; set; } = [];
        public ICollection<CreditTransaction> CreditTransactions { get; set; } = [];
        public EscrowHold? EscrowHold { get; set; }
        public ICollection<Rating> Ratings { get; set; } = [];
    }
}
