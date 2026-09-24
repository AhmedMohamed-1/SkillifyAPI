namespace SkillifyAPI.Models
{
    public class UserBadge
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BadgeId { get; set; }
        public DateTime AwardedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Badge Badge { get; set; } = null!;
    }
}
