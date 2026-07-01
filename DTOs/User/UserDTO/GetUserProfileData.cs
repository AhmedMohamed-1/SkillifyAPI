using SkillifyAPI.DTOs.Badge.BadgeDTO;
using SkillifyAPI.DTOs.Language.LanguageDTO;
using SkillifyAPI.DTOs.Rating;
using SkillifyAPI.DTOs.Skill.SkillDTO;

namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Current user's own profile view data.
    /// </summary>
    public class GetUserProfileData
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? JobTitle { get; set; }
        public int CreditBalance { get; set; }
        public ICollection<GetBadgeDTO> Badges { get; set; } = [];
        public ICollection<GetLanguageDTO> Languages { get; set; } = [];

        public GetUserSkillDTO? OfferedSkill { get; set; }
        public ICollection<GetUserSkillDTO> NeededSkills { get; set; } = [];

        public string CompletedSessions { get; set; } = null!;
        public ICollection<GetReceivedReviewDTO> ReceivedReviews { get; set; } = [];
        public decimal? OverallRatingScore { get; set; }
    }
}
