namespace SkillifyAPI.Models
{
    public enum SkillType { Offered, Needed }

    public class UserSkill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; } = null!;
        public SkillType SkillType { get; set; } = SkillType.Offered;  
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public MainSkill Category { get; set; } = null!;
        public ICollection<UserSkillSubSkill> SubSkills { get; set; } = [];
    }
}
