namespace SkillifyAPI.ZegoService
{
    public interface IZegoTokenService
    {
        uint AppId { get; }
        string GenerateToken(string userId, DateTime endsAt);
    }
}
