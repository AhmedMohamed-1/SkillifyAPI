namespace SkillifyAPI.Models
{
    public class UserLanguage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LanguageId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Language Language { get; set; } = null!;
    }
}
