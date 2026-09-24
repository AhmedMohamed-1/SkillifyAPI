namespace SkillifyAPI.Models
{
    public class SubSkill
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IconKey { get; set; }
        public int MainSkillId { get; set; }


        // Navigation
        public MainSkill MainSkill { get; set; } = null!;
    }
}
