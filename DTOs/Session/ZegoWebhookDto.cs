using System.Text.Json.Serialization;

namespace SkillifyAPI.DTOs.Session
{
    /// <summary>
    /// Payload sent by ZegoCloud to our webhook endpoint for room-user events.
    /// </summary>
    public class ZegoWebhookDto
    {
        /// <summary>Event name, e.g. "room_user_join" or "room_user_leave".</summary>
        [JsonPropertyName("event")]
        public string Event { get; set; } = default!;

        /// <summary>The Zego room identifier (matches Session.ZegoRoomId).</summary>
        [JsonPropertyName("room_id")]
        public string RoomId { get; set; } = default!;

        /// <summary>
        /// The Zego user account string that joined/left.
        /// We store "userId" as the account so we can parse it back to an int.
        /// </summary>
        [JsonPropertyName("user_account")]
        public string UserAccount { get; set; } = default!;

        /// <summary>Unix timestamp (ms) of when the user joined the room.</summary>
        [JsonPropertyName("login_time")]
        public long? LoginTime { get; set; }

        /// <summary>Unix timestamp (ms) of when the user left the room.</summary>
        [JsonPropertyName("logout_time")]
        public long? LogoutTime { get; set; }

        /// <summary>HMAC-MD5 signature for request validation.</summary>
        [JsonPropertyName("signature")]
        public string Signature { get; set; } = default!;

        /// <summary>Unix timestamp used for signature generation.</summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>Random nonce used for signature generation.</summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; } = default!;
    }
}
