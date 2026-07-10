using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SkillifyAPI.CloudinaryService;
using SkillifyAPI.DTOs.User.UserDTO;
using SkillifyAPI.Helper;
using SkillifyAPI.JwtService;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.UserRepository;

namespace SkillifyAPI.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IJwtTokenService _jwt;
        private readonly IConfiguration _cfg;
        private readonly ILogger<UserService> _logger;
        private readonly IValidator<SignInDTO> _loginValidator;
        private readonly IValidator<RegisterDTO> _registerValidator;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IValidator<CompleteProfileDTO> _completeProfileValidator;

        public UserService(
            IValidator<SignInDTO> loginValidator,
            IValidator<RegisterDTO> registerValidator,
            IValidator<CompleteProfileDTO> completeProfileValidator,
            ILogger<UserService> logger,
            ICloudinaryService cloudinaryService,
            IUserRepository repo,
            IJwtTokenService jwt,
            IConfiguration cfg)
        {
            _repo = repo;
            _jwt = jwt;
            _cfg = cfg;
            _logger = logger;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _completeProfileValidator = completeProfileValidator;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<AuthResponseDto> LoginAsync(SignInDTO dto, CancellationToken ct = default)
        {
            _logger.LogInformation("auth.login_requested {Service} {Email}", nameof(UserService), dto.Email);
            ValidationHelper.EnsureValid(_loginValidator, dto);

            var hasher = new PasswordHasher<User>();
            var email = EmailNormalizer.NormalizeEmail(dto.Email);
            var user = await _repo.GetUserByEmailAsync(email, ct);

            if (user == null || hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("auth.login_failed {Service} {Email} {Reason}", nameof(UserService), dto.Email, "invalid_credentials");
                throw new SecurityTokenException("Invalid credentials.");
            }

            return await IssueTokensAsync(user, ct);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDTO dto, CancellationToken ct = default)
        {
            _logger.LogInformation("auth.register_requested {Service} {Email}", nameof(UserService), dto.Email);
            ValidationHelper.EnsureValid(_registerValidator, dto);

            var hasher = new PasswordHasher<User>();
            var email = EmailNormalizer.NormalizeEmail(dto.Email);

            if (await _repo.GetUserByEmailAsync(email, ct) != null)
            {
                _logger.LogWarning("auth.register_failed {Service} {Email} {Reason}", nameof(UserService), dto.Email, "email_exists");
                throw new SecurityTokenException("Email already exists.");
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                CreatedAt = now,
                UpdatedAt = now
            };
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            await _repo.SignUpAsync(user, ct);
            await _repo.SaveChangesAsync(ct);

            user = (await _repo.GetUserByEmailAsync(email, ct))!;
            _logger.LogInformation("auth.register_succeeded {Service} {UserId} {Email}", nameof(UserService), user.Id, user.Email);

            return await IssueTokensAsync(user, ct);
        }

        public async Task<AuthResponseDto> RefreshAsync(string token, CancellationToken ct = default)
        {
            _logger.LogInformation("auth.refresh_requested {Service}", nameof(UserService));
            var existing = await _repo.GetRefreshTokenAsync(token, ct);

            if (existing == null)
                throw new SecurityTokenException("Invalid refresh token.");
            if (existing.IsRevoked)
                throw new SecurityTokenException("Refresh token revoked.");
            if (existing.ExpiresAt <= DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token expired.");

            var user = existing.User ?? throw new SecurityTokenException("User not found for refresh token.");
            var refreshDays = int.Parse(_cfg["Jwt:RememberMeRefreshTokenExpirationDays"] ?? "30");
            var newRefresh = _jwt.CreateRefreshToken(user.Id, TimeSpan.FromDays(refreshDays));

            existing.IsRevoked = true;
            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokeReason = "Rotated on refresh";
            existing.ReplacedByToken = newRefresh.Token;

            await _repo.AddRefreshTokenAsync(newRefresh, ct);
            _repo.UpdateRefreshToken(existing);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("auth.refresh_succeeded {Service} {UserId}", nameof(UserService), user.Id);
            return BuildAuthResponse(_jwt.GenerateAccessToken(user, newRefresh.ToString()!), newRefresh, user.ProfileCompleted);
        }

        public async Task RevokeAsync(string refreshToken, string? reason = null, CancellationToken ct = default)
        {
            var existing = await _repo.GetRefreshTokenAsync(refreshToken, ct);
            if (existing == null)
                return;

            existing.IsRevoked = true;
            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokeReason = reason ?? "Revoked by user";

            _repo.UpdateRefreshToken(existing);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SignOutAsync(int userId, string? refreshToken = null, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await _repo.GetRefreshTokenAsync(refreshToken, ct);
                if (token != null)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokeReason = "User signed out";
                    _repo.UpdateRefreshToken(token);
                }
            }
            else
            {
                await _repo.RevokeAllForUserAsync(userId, "User signed out globally", ct);
            }
            await _repo.SaveChangesAsync(ct);
        }

        public async Task<GetUserProfileData> GetProfileAsync(int userId, CancellationToken ct = default)
        {
            var user = await _repo.GetUserForProfileAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            var completedCount = await _repo.GetCompletedSessionsCountAsync(userId, ct);
            return UserProfileMapper.ToProfileDto(user, completedCount);
        }

        public async Task<GetUserProfileData> CompleteProfileAsync(int userId, CompleteProfileDTO dto, CancellationToken ct = default)
        {
            ValidationHelper.EnsureValid(_completeProfileValidator, dto);

            var user = await _repo.GetUserByIdAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found.");

            await ValidateSkillSelectionAsync(dto.OfferedMainSkill, dto.OfferedSubSkills, ct);
            foreach (var needed in dto.NeededSkills)
                await ValidateSkillSelectionAsync(needed.MainSkillId, needed.SubSkillIds, ct);

            List<int>? languageIds = null;

            if (dto.LanguageIds is not null)
            {
                languageIds = dto.LanguageIds.Distinct().ToList();
                if (languageIds.Count > 0 && !await _repo.LanguagesExistAsync(languageIds, ct))
                {
                    throw new InvalidOperationException("One or more selected languages are invalid.");
                }
            }

            // ?? Handle Profile Picture ???????????????????????????????
            if (dto.ProfilePicture is not null)
            {
                // Delete old picture from Cloudinary if exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePublicId))
                    await _cloudinaryService.DeleteImageAsync(user.ProfilePicturePublicId);

                // Upload new picture
                var uploaded = await _cloudinaryService.UploadImageAsync(
                    dto.ProfilePicture,
                    folder: "SkillifyAPI/avatars");

                user.ProfilePictureUrl = uploaded.SecureUrl;
                user.ProfilePicturePublicId = uploaded.PublicId;
            }

            user.Bio = dto.Bio;
            user.JobTitle = dto.JobTitle;
            user.ProfileCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;


            await _repo.RemoveUserSkillsAsync(userId, ct);

            var userSkills = new List<UserSkill>
            {
                BuildUserSkill(userId, dto.OfferedMainSkill, dto.OfferedSubSkills, dto.OfferedDescription, SkillType.Offered)
            };

            foreach (var needed in dto.NeededSkills)
            {
                userSkills.Add(BuildUserSkill(userId, needed.MainSkillId, needed.SubSkillIds, needed.Description, SkillType.Needed));
            }

            await _repo.AddUserSkillsAsync(userSkills, ct);

            if (languageIds is not null)
            {
                await _repo.RemoveUserLanguagesAsync(userId, ct);

                await _repo.AddUserLanguagesAsync(
                    languageIds.Select(id => new UserLanguage
                    {
                        UserId = userId,
                        LanguageId = id
                    }),
                    ct);
            }

            await _repo.SaveChangesAsync(ct);

            return await GetProfileAsync(userId, ct);
        }

        public async Task<SkillifyAPI.DTOs.PagedResult<UsersListDTO>> GetUsersAsync(int page, int pageSize, string? name = null, int? skillId = null, decimal? minRating = null, int? langId = null, CancellationToken ct = default)
        {
            var (users, totalCount) = await _repo.GetUsersPagedAsync(page, pageSize, name, skillId, minRating, langId, ct);
            return new SkillifyAPI.DTOs.PagedResult<UsersListDTO>(users.Select(UserProfileMapper.ToListItemDto), totalCount);
        }

        private async Task ValidateSkillSelectionAsync(int mainSkillId, int[] subSkillIds, CancellationToken ct = default)
        {
            if (!await _repo.MainSkillExistsAsync(mainSkillId, ct))
                throw new InvalidOperationException("Main skill does not exist.");

            if (!await _repo.SubSkillsExistForMainSkillAsync(mainSkillId, subSkillIds, ct))
                throw new InvalidOperationException("One or more sub-skills are invalid for the selected main skill.");
        }

        private static UserSkill BuildUserSkill(int userId, int mainSkillId, int[] subSkillIds, string? description, SkillType skillType)
        {
            var userSkill = new UserSkill
            {
                UserId = userId,
                CategoryId = mainSkillId,
                Description = description ?? string.Empty,
                SkillType = skillType,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var subSkillId in subSkillIds.Distinct())
            {
                userSkill.SubSkills.Add(new UserSkillSubSkill { SubSkillId = subSkillId });
            }

            return userSkill;
        }

        private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken ct = default)
        {
            var refreshDays = int.Parse(_cfg["Jwt:RememberMeRefreshTokenExpirationDays"] ?? "30");
            var refreshToken = _jwt.CreateRefreshToken(user.Id, TimeSpan.FromDays(refreshDays));

            await _repo.AddRefreshTokenAsync(refreshToken, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("auth.login_succeeded {Service} {UserId}", nameof(UserService), user.Id);
            return BuildAuthResponse(_jwt.GenerateAccessToken(user, refreshToken.Token), refreshToken, user.ProfileCompleted);
        }

        private AuthResponseDto BuildAuthResponse(string accessToken, RefreshToken refreshToken, bool profileCompleted)
        {
            var accessMinutes = int.Parse(_cfg["Jwt:AccessTokenExpirationMinutes"] ?? "15");
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresInSeconds = accessMinutes * 60,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessMinutes),
                profileCompleted = profileCompleted
            };
        }
    }
}
