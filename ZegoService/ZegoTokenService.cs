using ZegoServerAssistant;

namespace SkillifyAPI.ZegoService
{
    public class ZegoTokenService : IZegoTokenService
    {
        private readonly uint _appId;
        private readonly string _serverSecret;

        public ZegoTokenService(IConfiguration config)
        {
            _appId = uint.Parse(config["Zego:AppId"]!);
            _serverSecret = config["Zego:ServerSecret"]!;
        }

        public uint AppId => _appId;

        public string GenerateToken(string userId, DateTime endsAt)
        {
            var effectiveSeconds = Math.Max((long)(endsAt - DateTime.UtcNow).TotalSeconds, 60);

            // UIKit requires an identity token with empty payload (room/user come from kit token).
            var result = ServerAssistant.GenerateToken04(
                _appId, userId, _serverSecret, effectiveSeconds, "");

            if (result.errorInfo.errorCode != ErrorCode.success)
                throw new InvalidOperationException(
                    $"Failed to generate Zego token: {result.errorInfo.errorMessage}");

            return result.token;
        }
    }
}
