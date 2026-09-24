using SkillifyAPI.DTOs.Skill.SkillDTO;

namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Request payload used to complete or partially update user profile information.
    /// Omit a property (leave it null) to keep the current value unchanged.
    /// </summary>
    public class CompleteProfileDTO
    {
        public string? Bio { get; set; }
        public string? JobTitle { get; set; }
        public string? FullName { get; set; }
        public int? OfferedMainSkill { get; set; }
        public int[]? OfferedSubSkills { get; set; }
        public string? OfferedDescription { get; set; }
        public UserSkillSelectionDTO[]? NeededSkills { get; set; }
        public List<int>? LanguageIds { get; set; }

        public bool HasAnyUpdate =>
            Bio is not null ||
            JobTitle is not null ||
            FullName is not null ||
            OfferedMainSkill.HasValue ||
            OfferedSubSkills is not null ||
            OfferedDescription is not null ||
            NeededSkills is not null ||
            LanguageIds is not null;

        public bool IsUpdatingOffered =>
            OfferedMainSkill.HasValue ||
            OfferedSubSkills is not null ||
            OfferedDescription is not null;
    }
}
