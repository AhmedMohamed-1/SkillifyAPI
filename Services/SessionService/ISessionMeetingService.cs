using SkillifyAPI.DTOs.Session;
using SkillifyAPI.Models;

namespace SkillifyAPI.Services.SessionService
{
    public interface ISessionMeetingService
    {
        public Task OnSessionAccepted(int sessionId);
        public Task OpenSession(int sessionId);
        public Task CloseSession(int sessionId);
        public void CancelScheduledJobs(Session session);

        /// <summary>
        /// Processes a ZegoCloud room-user event webhook.
        /// On "room_user_join" by the helper  → releases escrow to the helper immediately.
        /// On session timeout without helper join → marks session Expired and refunds the payer.
        /// </summary>
        Task HandleWebhookAsync(ZegoWebhookDto dto, CancellationToken ct = default);
        Task<GetSessionDTO> RequestSessionAsync(int requesterId, RequestHelpDTO dto, CancellationToken ct = default);
        Task<GetSessionDTO> OfferSessionAsync(int helperId, OfferHelpDTO dto, CancellationToken ct = default);
        Task AcceptSessionAsync(int userId, int sessionId, CancellationToken ct = default);
        Task DeclineSessionAsync(int helperId, int sessionId, CancellationToken ct = default);
        Task CancelSessionAsync(int userId, int sessionId, CancellationToken ct = default);
        Task RescheduleSessionAsync(int userId, int sessionId, DateTime newScheduledAt, string? comment = null, CancellationToken ct = default);
        Task<GetSessionDTO> GetSessionByIdAsync(int userId, int sessionId, CancellationToken ct = default);
        Task<IEnumerable<GetSessionDTO>> GetRequestedSessionsAsync(int requesterId, CancellationToken ct = default);
        Task<IEnumerable<GetSessionDTO>> GetReceivedSessionsAsync(int helperId, CancellationToken ct = default);
    }
}
