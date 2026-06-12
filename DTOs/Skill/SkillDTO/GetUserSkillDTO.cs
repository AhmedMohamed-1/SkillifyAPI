namespace SkillifyAPI.DTOs.Skill.SkillDTO
{
    /// <summary>
    /// A user's offered or needed skill with catalog details (ready for icons via IconKey).
    /// </summary>
    public class GetUserSkillDTO
    {
        public int UserSkillId { get; set; }
        public GetMainSkillDTO MainSkill { get; set; } = null!;
        public ICollection<GetSubSkillDTO> SubSkills { get; set; } = [];
        public string Description { get; set; } = null!;
    }
}
