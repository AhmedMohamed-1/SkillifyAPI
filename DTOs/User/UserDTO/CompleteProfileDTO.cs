namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Request payload used to complete user profile information.
    /// </summary>
    public class CompleteProfileDTO
    {
        public IFormFile? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public string? JobTitle { get; set; }

        public int OfferedMainSkill { get; set; }
        public int[] OfferedSubSkills { get; set; } = null!;
        public string? OfferedDescription { get; set; }

        public int NeededMainSkill { get; set; }
        public int[] NeededSubSkills { get; set; } = null!;
        public string? NeededDescription { get; set; }
    }
}
