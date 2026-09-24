using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs.Notification;
using SkillifyAPI.Services.NotificationService;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace SkillifyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get my notifications",
            Description = "Retrieves a list of all notifications for the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NotificationDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyNotifications(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetMyNotificationsAsync(userId, ct);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching notifications.", details = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        [SwaggerOperation(
            Summary = "Get unread notifications count",
            Description = "Retrieves the count of unread notifications for the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnreadCountDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId, ct);
                return Ok(unreadCount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching the unread count.", details = ex.Message });
            }
        }

        [HttpPost("devices")]
        [SwaggerOperation(
            Summary = "Register device for push notifications",
            Description = "Registers or reactivates an FCM token for the authenticated user's device."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.FcmToken))
                return BadRequest(new { message = "FCM token is required." });

            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.RegisterDeviceAsync(userId, dto.FcmToken.Trim(), ct);
                return Ok(new { message = "Device registered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while registering the device.", details = ex.Message });
            }
        }

        [HttpDelete("devices")]
        [SwaggerOperation(
            Summary = "Unregister device from push notifications",
            Description = "Deactivates an FCM token for the authenticated user's device."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnregisterDevice([FromBody] RegisterDeviceDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.FcmToken))
                return BadRequest(new { message = "FCM token is required." });

            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.UnregisterDeviceAsync(userId, dto.FcmToken.Trim(), ct);
                return Ok(new { message = "Device unregistered successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while unregistering the device.", details = ex.Message });
            }
        }

        [HttpPut("{id}/read")]
        [SwaggerOperation(
            Summary = "Mark notification as read",
            Description = "Marks a specific notification as read."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAsReadAsync(userId, id, ct);
                return Ok(new { message = "Notification marked as read." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while marking the notification as read.", details = ex.Message });
            }
        }

        [HttpPut("read-all")]
        [SwaggerOperation(
            Summary = "Mark all notifications as read",
            Description = "Marks all unread notifications for the currently authenticated user as read."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadAsync(userId, ct);
                return Ok(new { message = "All notifications marked as read." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while marking notifications as read.", details = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid user token or user is not logged in.");

            return userId;
        }
    }
}
