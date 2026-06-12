using System.Collections.Generic;

namespace SkillifyAPI.DTOs.Skill.SkillDTO
{
    public class GetMainSkillWithSubSkillsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? IconKey { get; set; }
        public List<GetSubSkillDTO> SubSkills { get; set; } = [];
    }
}
