using System.Security.Cryptography;
using System.Text;

namespace SkillifyAPI.ZegoService
{
    public class ZegoRoomService : IZegoRoomService
    {
        private readonly uint _appId;
        private readonly string _serverSecret;
        private readonly IHttpClientFactory _httpClientFactory;

        public ZegoRoomService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _appId = uint.Parse(config["Zego:AppId"]!);
            _serverSecret = config["Zego:ServerSecret"]!;
            _httpClientFactory = httpClientFactory;
        }

        public async Task CloseRoomAsync(string roomId, CancellationToken ct = default)
        {
            var signatureNonce = GenerateSignatureNonce();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(_appId, signatureNonce, _serverSecret, timestamp);

            var url =
                $"https://rtc-api.zego.im/?Action=CloseRoom" +
                $"&AppId={_appId}" +
                $"&Signature={signature}" +
                $"&SignatureNonce={signatureNonce}" +
                $"&SignatureVersion=2.0" +
                $"&Timestamp={timestamp}" +
                $"&RoomId={Uri.EscapeDataString(roomId)}" +
                $"&RoomCloseCallback=false";

            var client = _httpClientFactory.CreateClient();
            await client.GetAsync(url, ct);
        }

        private static string GenerateSignatureNonce()
        {
            var bytes = new byte[8];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string GenerateSignature(uint appId, string signatureNonce, string serverSecret, long timestamp)
        {
            var input = $"{appId}{signatureNonce}{serverSecret}{timestamp}";
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
