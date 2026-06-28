using FirebaseAdmin.Messaging;
using SkillifyAPI.Repositories.UserDeviceRepository;

namespace SkillifyAPI.Firebase
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly IUserDeviceRepository _userDeviceRepository;

        public FirebaseNotificationService(IUserDeviceRepository userDeviceRepository)
        {
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task SendAsync(string token, string title, string body, CancellationToken ct = default)
        {
            var message = new Message
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            }
            catch (FirebaseMessagingException ex) when (IsInvalidToken(ex))
            {
                await _userDeviceRepository.DeactivateTokenAsync(token, ct);
                await _userDeviceRepository.SaveChangesAsync(ct);
            }
        }

        public async Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default)
        {
            var tokens = await _userDeviceRepository.GetActiveTokensByUserIdAsync(userId, ct);
            if (tokens.Count == 0)
                return;

            var messages = tokens.Select(token => new Message
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            }).ToList();

            var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages, ct);

            for (var i = 0; i < response.Responses.Count; i++)
            {
                if (response.Responses[i].IsSuccess)
                    continue;

                var exception = response.Responses[i].Exception;
                if (exception is FirebaseMessagingException messagingEx && IsInvalidToken(messagingEx))
                    await _userDeviceRepository.DeactivateTokenAsync(tokens[i], ct);
            }

            await _userDeviceRepository.SaveChangesAsync(ct);
        }

        private static bool IsInvalidToken(FirebaseMessagingException ex)
            => ex.MessagingErrorCode is MessagingErrorCode.Unregistered
                or MessagingErrorCode.InvalidArgument;
    }
}
