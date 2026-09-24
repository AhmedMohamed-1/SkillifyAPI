namespace SkillifyAPI.ZegoService
{
    public interface IZegoRoomService
    {
        Task CloseRoomAsync(string roomId, CancellationToken ct = default);
    }
}
