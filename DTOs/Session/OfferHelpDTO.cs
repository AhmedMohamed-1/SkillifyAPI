using SkillifyAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SkillifyAPI.DTOs.Session
{
    public class OfferHelpDTO
    {
        [Required]
        public int RequesterId { get; set; }

        [Required]
        public int MainSkillId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Topic { get; set; } = null!;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string ProblemDescription { get; set; } = null!;

        [Required]
        public SessionDuration DurationMinutes { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }
    }
}
