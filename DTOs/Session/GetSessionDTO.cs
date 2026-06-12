using SkillifyAPI.Models;

namespace SkillifyAPI.DTOs.Session
{
    public class GetSessionDTO
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string RequesterName { get; set; } = null!;
        public int HelperId { get; set; }
        public string HelperName { get; set; } = null!;
        public int MainSkillId { get; set; }
        public string MainSkillName { get; set; } = null!;
        public string Topic { get; set; } = null!;
        public string ProblemDescription { get; set; } = null!;
        public int DurationMinutes { get; set; }
        public int CreditCost { get; set; }
        public SessionStatus Status { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ZegoRoomId { get; set; }
    }
}
