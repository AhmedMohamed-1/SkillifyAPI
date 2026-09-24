namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Authentication response returned after register, login, or refresh operations.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Signed JWT access token used for authenticated API calls.
        /// </summary>
        public string AccessToken { get; set; } = null!;
        /// <summary>
        /// Refresh token used to obtain a new access token.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
        /// <summary>
        /// Access token lifetime in seconds.
        /// </summary>
        public int AccessTokenExpiresInSeconds { get; set; }
        /// <summary>
        /// UTC date and time when the refresh token expires.
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }
        /// <summary>
        /// UTC date and time when the access token expires.
        /// </summary>
        public DateTime? AccessTokenExpiresAt { get; set; }
        /// <summary>
        /// profile completion status of the user
        ///</summary>
        public bool profileCompleted { get; set; }
    }
}
