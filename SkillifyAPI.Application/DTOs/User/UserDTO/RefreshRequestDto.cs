namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Request payload used to rotate access and refresh tokens.
    /// </summary>
    public class RefreshRequestDto
    {
        /// <summary>
        /// Refresh token value. Can be omitted when cookie mode is enabled.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
    }
}
