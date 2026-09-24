namespace SkillifyAPI.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;

        // Navigation
        public ICollection<UserLanguage> UserLanguages { get; set; } = [];
    }
}
