namespace SkillifyAPI.Firebase
{
    public interface IFirebaseNotificationService
    {
        Task SendAsync(string token, string title, string body, CancellationToken ct = default);

        Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default);
    }
}
