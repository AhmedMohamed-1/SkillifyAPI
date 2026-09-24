namespace SkillifyAPI.DTOs.User.UserDTO
{
    /// <summary>
    /// Request payload used to register a new user account.
    /// </summary>
    public class RegisterDTO
    {
        /// <summary>
        /// User fullname.
        /// </summary>
        public string FullName { get; set; } = null!;
        /// <summary>
        /// User login email address. Must be unique.
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// Plain-text password that will be hashed before storage.
        /// </summary>
        public string Password { get; set; } = null!;
        /// <summary>
        /// Password confirmation. Must match <see cref="Password"/>.
        /// </summary>
        public string ConfirmPassword { get; set; } = null!;
    }
}
