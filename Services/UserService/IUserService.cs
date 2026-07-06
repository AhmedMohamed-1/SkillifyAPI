using SkillifyAPI.DTOs;
using SkillifyAPI.DTOs.User.UserDTO;

namespace SkillifyAPI.Services.UserService
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDTO dto, CancellationToken ct = default);
        Task<AuthResponseDto> LoginAsync(SignInDTO dto, CancellationToken ct = default);
        Task<AuthResponseDto> RefreshAsync(string token, CancellationToken ct = default);
        Task RevokeAsync(string refreshToken, string? reason = null, CancellationToken ct = default);
        Task SignOutAsync(int userId, string? refreshToken = null, CancellationToken ct = default);
        Task<GetUserProfileData> GetProfileAsync(int userId, CancellationToken ct = default);
        Task<GetUserProfileData> CompleteProfileAsync(int userId, CompleteProfileDTO dto, CancellationToken ct = default);
        Task<PagedResult<UsersListDTO>> GetUsersAsync(int page, int pageSize, string? name = null, int? skillId = null, decimal? minRating = null, int? langId = null, CancellationToken ct = default);
    }
}
