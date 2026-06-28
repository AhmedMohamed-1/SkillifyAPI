namespace SkillifyAPI.Models
{
    public class UserDevice
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string FcmToken { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsActive { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
