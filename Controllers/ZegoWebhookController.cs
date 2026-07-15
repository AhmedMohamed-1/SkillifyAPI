using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs.Session;
using SkillifyAPI.Services.SessionService;

namespace SkillifyAPI.Controllers;

[ApiController]
[Route("api/zego/webhook")]
public class ZegoWebhookController : ControllerBase
{
    private static readonly object LatestCallbackLock = new();
    private static ZegoWebhookDto? _latestCallback;
    private static DateTime? _latestCallbackReceivedAt;

    private readonly ISessionMeetingService _meetingService;
    private readonly ILogger<ZegoWebhookController> _logger;

    public ZegoWebhookController(
        ISessionMeetingService meetingService,
        ILogger<ZegoWebhookController> logger)
    {
        _meetingService = meetingService;
        _logger = logger;
    }

    /// <summary>
    /// Receives ZegoCloud room-user events (room_user_join / room_user_leave).
    ///
    /// Trigger logic:
    ///   - Helper joins before the halfway point  → escrow released, credits transferred to helper.
    ///   - Helper never joins (session times out) → session marked Expired, credits refunded to payer.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Callback(
        [FromBody] ZegoWebhookDto dto,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Zego webhook received: Event={Event}, RoomId={RoomId}, User={User}",
            dto.Event, dto.RoomId, dto.UserAccount);

        lock (LatestCallbackLock)
        {
            _latestCallback = dto;
            _latestCallbackReceivedAt = DateTime.UtcNow;
        }

        await _meetingService.HandleWebhookAsync(dto, ct);

        return Ok();
    }

    /// <summary>
    /// Returns the most recent Zego webhook payload received (for debugging).
    /// </summary>
    [HttpGet("latest")]
    public IActionResult GetLatestCallback()
    {
        lock (LatestCallbackLock)
        {
            if (_latestCallback is null)
            {
                return NotFound(new { message = "No Zego webhook received yet." });
            }

            return Ok(new
            {
                receivedAt = _latestCallbackReceivedAt,
                payload = _latestCallback
            });
        }
    }
}