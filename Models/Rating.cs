using System.ComponentModel.DataAnnotations.Schema;

namespace SkillifyAPI.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int ReviewerId { get; set; }
        public int RevieweeId { get; set; }
        public decimal Score { get; set; } // 1.0 – 5.0
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Session Session { get; set; } = null!;

        [ForeignKey(nameof(ReviewerId))]
        [InverseProperty(nameof(User.GivenRatings))]
        public User Reviewer { get; set; } = null!;

        [ForeignKey(nameof(RevieweeId))]
        [InverseProperty(nameof(User.ReceivedRatings))]
        public User Reviewee { get; set; } = null!;
    }
}
