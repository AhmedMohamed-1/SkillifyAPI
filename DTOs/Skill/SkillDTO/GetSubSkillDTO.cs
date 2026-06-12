namespace SkillifyAPI.DTOs.Skill.SkillDTO
{
    public class GetSubSkillDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IconKey { get; set; }
    }
}
