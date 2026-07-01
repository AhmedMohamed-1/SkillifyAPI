namespace SkillifyAPI.DTOs.Skill.SkillDTO
{
    /// <summary>
    /// A main skill with its selected sub-skills and optional description.
    /// </summary>
    public class UserSkillSelectionDTO
    {
        public int MainSkillId { get; set; }
        public int[] SubSkillIds { get; set; } = null!;
        public string? Description { get; set; }
    }
}
