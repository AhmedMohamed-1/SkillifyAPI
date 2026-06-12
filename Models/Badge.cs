namespace SkillifyAPI.Models
{
    public enum BadgeCriteriaType
    {
        SessionCount,
        AverageRating,
        ConsistentHelping
    }

    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconKey { get; set; }
        public BadgeCriteriaType CriteriaType { get; set; }
        public int CriteriaThreshold { get; set; }

        // Navigation
        public ICollection<UserBadge> UserBadges { get; set; } = [];
    }
}
