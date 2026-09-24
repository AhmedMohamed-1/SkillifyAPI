namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Request payload used to authenticate an existing user.
    /// </summary>
    public class SignInDTO
    {
        /// <summary>
        /// User login email address.
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// User plain-text password.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
