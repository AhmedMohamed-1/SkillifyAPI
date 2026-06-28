using System.ComponentModel.DataAnnotations;

namespace SkillifyAPI.DTOs.Session
{
    public class RescheduleSessionDTO
    {
        [Required]
        public DateTime NewScheduledAt { get; set; }

        public string? Comment { get; set; }
    }
}
