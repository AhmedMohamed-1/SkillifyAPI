namespace SkillifyAPI.DTOs.Badge.BadgeDTO
{
    public class GetBadgeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconKey { get; set; }
    }
}
