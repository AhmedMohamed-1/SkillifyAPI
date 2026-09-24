namespace SkillifyAPI.Models
{
    public class UserSkillSubSkill
    {
        public int UserSkillId { get; set; } // PK 
        public UserSkill UserSkill { get; set; } = null!;

        public int SubSkillId { get; set; } // FK
        public SubSkill SubSkill { get; set; } = null!;
    }
}
