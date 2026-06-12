namespace SkillifyAPI.Models
{
    public enum PushPlatform { iOS, Android }

    public class PushToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public PushPlatform Platform { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
