using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using SkillifyAPI.DTOs.User.UserDTO;
using SkillifyAPI.Services.UserService;
using Swashbuckle.AspNetCore.Annotations;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling user registration, authentication, token management, and profile operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">The user service instance.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="dto">The registration details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication response containing JWT tokens.</returns>
        /// <response code="200">User registered successfully and tokens returned.</response>
        /// <response code="400">Invalid registration details or email already exists.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user profile with credentials, hashes the password, and returns access and refresh tokens."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDTO dto, CancellationToken ct)
        {
            try
            {
                var result = await _userService.RegisterAsync(dto, ct);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during registration."  + ex.Message});
            }
        }

        /// <summary>
        /// Authenticates an existing user.
        /// </summary>
        /// <param name="dto">The login credentials.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication response containing JWT tokens.</returns>
        /// <response code="200">User authenticated successfully and tokens returned.</response>
        /// <response code="401">Invalid email or password credentials.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "User login",
            Description = "Authenticates a user using email and password, returning a new set of JWT access and refresh tokens."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] SignInDTO dto, CancellationToken ct)
        {
            try
            {
                var result = await _userService.LoginAsync(dto, ct);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during login." + ex.Message});
            }
        }

        /// <summary>
        /// Refreshes the JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="dto">The refresh token request payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>New authentication response containing updated JWT tokens.</returns>
        /// <response code="200">Access token refreshed successfully.</response>
        /// <response code="401">The refresh token is invalid, expired, or revoked.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("refresh")]
        [SwaggerOperation(
            Summary = "Refresh JWT access token",
            Description = "Validates the provided refresh token and issues a new access token and rotating refresh token."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _userService.RefreshAsync(dto.RefreshToken, ct);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while refreshing token." + ex.Message});
            }
        }

        /// <summary>
        /// Signs out the authenticated user from all devices by revoking all their active refresh tokens.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">All refresh tokens revoked successfully. User is signed out from all devices.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("revoke")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Sign out from all devices",
            Description = "Revokes all active refresh tokens for the authenticated user, effectively signing them out from every device and session."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> Revoke(CancellationToken ct)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.SignOutAsync(userId, refreshToken: null, ct); // revoke ALL
                return NoContent();
            }
            catch (SecurityTokenException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while revoking token." + ex.Message });
            }
        }

        /// <summary>
        /// Logs out the currently authenticated user by invalidating their refresh tokens.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Logged out successfully.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="404">User profile or session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Log out user",
            Description = "Logs out the authenticated user by invalidating the current session's access and refresh tokens."
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var refreshToken = User.FindFirstValue("sid");
                await _userService.SignOutAsync(userId, refreshToken, ct);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during logout." + ex.Message});
            }
        }

        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The profile data of the logged-in user.</returns>
        /// <response code="200">Profile data retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User profile not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get current user profile",
            Description = "Fetches the full profile details, including name, email, main skill, and sub-skills of the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserProfileData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetUserProfileData>> GetMyProfile(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _userService.GetProfileAsync(userId, ct);
                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while fetching profile." + ex.Message});
            }
        }

        /// <summary>
        /// Completes or updates the profile details of the authenticated user.
        /// </summary>
        /// <param name="dto">The profile completion details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated user profile data.</returns>
        /// <response code="200">Profile updated successfully.</response>
        /// <response code="400">Invalid profile data, main skill, or sub-skills selection.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User profile not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [Authorize]
        [HttpPut("me/profile")]
        [SwaggerOperation(
            Summary = "Complete or update user profile",
            Description = "Allows an authenticated user to complete or update their profile by setting bio, job title, main skill, sub-skills, and languages."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserProfileData))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetUserProfileData>> CompleteProfile([FromBody] CompleteProfileDTO dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedProfile = await _userService.CompleteProfileAsync(userId, dto, ct);
                return Ok(updatedProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while completing profile." + ex.Message });
            }
        }

        /// <summary>
        /// Updates the profile picture of the authenticated user.
        /// </summary>
        /// <param name="profilePicture">The new profile picture file.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated user profile data.</returns>
        /// <response code="200">Profile picture updated successfully.</response>
        /// <response code="400">No file provided or invalid file.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [Authorize]
        [Consumes("multipart/form-data")]
        [HttpPut("me/profile-picture")]
        [SwaggerOperation(
            Summary = "Update profile picture",
            Description = "Uploads a new profile picture for the authenticated user. Replaces the existing one on Cloudinary."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserProfileData))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetUserProfileData>> UpdateProfilePicture(IFormFile profilePicture, CancellationToken ct)
        {
            if (profilePicture == null || profilePicture.Length == 0)
                return BadRequest(new { message = "A valid profile picture file is required." });

            try
            {
                var userId = GetCurrentUserId();
                var updatedProfile = await _userService.UpdateProfilePictureAsync(userId, profilePicture, ct);
                return Ok(updatedProfile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating profile picture." + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of all users, optionally filtered by name, skill, minimum rating, and language.
        /// </summary>
        /// <param name="page">The page number, defaulting to 1.</param>
        /// <param name="pageSize">The number of users per page, defaulting to 20.</param>
        /// <param name="name">Optional user name to search for.</param>
        /// <param name="skillId">Optional ID of a main skill the user offers or needs.</param>
        /// <param name="minRating">Optional minimum average rating.</param>
        /// <param name="langId">Optional language ID the user knows.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated list of users.</returns>
        /// <response code="200">List of users retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get all users (paginated and filtered)",
            Description = "Retrieves a paginated list of registered users, with optional filtering."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UsersListDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<PagedResult<UsersListDTO>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? name = null, [FromQuery] int? skillId = null, [FromQuery] decimal? minRating = null, [FromQuery] int? langId = null, CancellationToken ct = default)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new { message = "Page and page size must be greater than or equal to 1." });
                }
                var usersResult = await _userService.GetUsersAsync(page, pageSize, name, skillId, minRating, langId, ct);
                return Ok(usersResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while retrieving users." + ex.Message});
            }
        }

        /// <summary>
        /// Retrieves the profile information of a specific user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The profile data of the specified user.</returns>
        /// <response code="200">Profile data retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User profile not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [Authorize]
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get user profile by ID",
            Description = "Fetches the full profile details of a specific user by their ID."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserProfileData))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetUserProfileData>> GetUserProfile(int id, CancellationToken ct)
        {
            try
            {
                var profile = await _userService.GetProfileAsync(id, ct);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while fetching profile." + ex.Message});
            }
        }

        private int GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid user token or user is not logged in.");

            return userId;
        }
    }
}
