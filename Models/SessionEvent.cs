namespace SkillifyAPI.Models
{
    public class SessionEvent
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public SessionStatus Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Comment { get; set; }

        // Navigation
        public Session Session { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
