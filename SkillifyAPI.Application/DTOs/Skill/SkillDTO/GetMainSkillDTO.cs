namespace SkillifyAPI.DTOs.Skill.SkillDTO
{
    public class GetMainSkillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? IconKey { get; set; }
    }
}
