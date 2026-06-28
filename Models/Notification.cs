using System.ComponentModel.DataAnnotations.Schema;

namespace SkillifyAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
