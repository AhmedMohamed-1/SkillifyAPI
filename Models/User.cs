using System.ComponentModel.DataAnnotations.Schema;

namespace SkillifyAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public string? ProfilePicturePublicId { get; set; }
        public string? Bio { get; set; }
        public string? JobTitle { get; set; }
        public int CreditBalance { get; set; } = 100;
        public bool ProfileCompleted { get; set; } = false;
        public DateTime? LastGiftCreditAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<UserLanguage> Languages { get; set; } = [];
        public ICollection<UserSkill> Skills { get; set; } = [];
        public ICollection<PushToken> PushTokens { get; set; } = [];
        public ICollection<UserDevice> Devices { get; set; } = [];
        public ICollection<UserBadge> Badges { get; set; } = [];
        public ICollection<CreditTransaction> CreditTransactions { get; set; } = [];
        public ICollection<EscrowHold> EscrowHolds { get; set; } = [];
        public ICollection<SessionEvent> SessionEvents { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];

        [InverseProperty(nameof(Session.Requester))]
        public ICollection<Session> RequestedSessions { get; set; } = [];

        [InverseProperty(nameof(Session.Helper))]
        public ICollection<Session> HelpedSessions { get; set; } = [];

        [InverseProperty(nameof(Rating.Reviewer))]
        public ICollection<Rating> GivenRatings { get; set; } = [];

        [InverseProperty(nameof(Rating.Reviewee))]
        public ICollection<Rating> ReceivedRatings { get; set; } = [];
    }
}
