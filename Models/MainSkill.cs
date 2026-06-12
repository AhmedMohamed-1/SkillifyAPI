namespace SkillifyAPI.Models
{
    public class MainSkill
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? IconKey { get; set; }

        // Navigation
        public ICollection<SubSkill> SubSkills { get; set; } = [];
    }
}
