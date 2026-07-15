using SkillifyAPI.DTOs.Skill.SkillDTO;

namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Lightweight user projection used in listing endpoints.
    /// </summary>
    public class UsersListDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? JobTitle { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public GetMainSkillDTO? OfferedMainSkill { get; set; }
        public ICollection<GetMainSkillDTO> NeededMainSkills { get; set; } = [];
        public decimal? OverallRatingScore { get; set; }
    }
}
