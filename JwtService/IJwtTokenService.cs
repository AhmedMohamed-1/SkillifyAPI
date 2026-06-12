using System.Security.Claims;
using SkillifyAPI.Models;

namespace SkillifyAPI.JwtService
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, string refreshToken);
        RefreshToken CreateRefreshToken(int userId, TimeSpan validity, string? ip = null);
        ClaimsPrincipal? ValidatePrincipalFromExpiredToken(string token);
    }
}

