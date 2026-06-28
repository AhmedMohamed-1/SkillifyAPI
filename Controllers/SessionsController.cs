using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.ZegoService;
using SkillifyAPI.DTOs.Session;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.SessionRepository;
using SkillifyAPI.Services.SessionService;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling session management, including requesting, offering, and managing sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionMeetingService _sessionMeetingService;
        private readonly ISessionRepository _sessionRepository;
        private readonly IZegoTokenService _zego;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class.
        /// </summary>
        /// <param name="sessionMeetingService">The session meeting service.</param>
        /// <param name="sessionRepository">The session repository.</param>
        /// <param name="zego">The Zego token service.</param>
        public SessionsController(
            ISessionMeetingService sessionMeetingService,
            ISessionRepository sessionRepository,
            IZegoTokenService zego)
        {
            _sessionMeetingService = sessionMeetingService;
            _sessionRepository = sessionRepository;
            _zego = zego;
        }

        /// <summary>
        /// Requests a help session from a helper.
        /// </summary>
        /// <param name="dto">The session request details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created session details.</returns>
        /// <response code="201">Session requested successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Target helper or skill not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("request")]
        [SwaggerOperation(
            Summary = "Request a help session",
            Description = "Creates a request for a help session from a specific helper."
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> RequestHelp([FromBody] RequestHelpDTO dto, CancellationToken ct)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                var session = await _sessionMeetingService.RequestSessionAsync(requesterId, dto, ct);
                return CreatedAtAction(nameof(GetSessionById), new { sessionId = session.Id }, session);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while requesting help.", details = ex.Message });
            }
        }

        /// <summary>
        /// Offers a help session to a requester.
        /// </summary>
        /// <param name="dto">The session offer details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created session details.</returns>
        /// <response code="201">Session offered successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">Target requester or skill not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("offer")]
        [SwaggerOperation(
            Summary = "Offer a help session",
            Description = "Creates an offer for a help session to a specific requester."
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> OfferHelp([FromBody] OfferHelpDTO dto, CancellationToken ct)
        {
            try
            {
                var helperId = GetCurrentUserId();
                var session = await _sessionMeetingService.OfferSessionAsync(helperId, dto, ct);
                return CreatedAtAction(nameof(GetSessionById), new { sessionId = session.Id }, session);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while offering help.", details = ex.Message });
            }
        }

        /// <summary>
        /// Accepts a pending session request or offer.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">Session accepted successfully.</response>
        /// <response code="400">Session cannot be accepted in its current state.</response>
        /// <response code="401">User is not authenticated or not authorized to accept this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("{sessionId}/accept")]
        [SwaggerOperation(
            Summary = "Accept a session",
            Description = "Accepts a pending session request or offer, making it active."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> AcceptSession(int sessionId, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _sessionMeetingService.AcceptSessionAsync(userId, sessionId, ct);
                return Ok(new { message = "Session accepted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while accepting the session.", details = ex.Message });
            }
        }

        /// <summary>
        /// Declines a pending session request or offer.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">Session declined successfully.</response>
        /// <response code="400">Session cannot be declined in its current state.</response>
        /// <response code="401">User is not authenticated or not authorized to decline this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("{sessionId}/decline")]
        [SwaggerOperation(
            Summary = "Decline a session",
            Description = "Declines a pending session request or offer."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> DeclineSession(int sessionId, CancellationToken ct)
        {
            try
            {
                var helperId = GetCurrentUserId();
                await _sessionMeetingService.DeclineSessionAsync(helperId, sessionId, ct);
                return Ok(new { message = "Session declined successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while declining the session.", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancels an accepted or pending session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">Session cancelled successfully.</response>
        /// <response code="400">Session cannot be cancelled in its current state.</response>
        /// <response code="401">User is not authenticated or not authorized to cancel this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("{sessionId}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel a session",
            Description = "Cancels a previously accepted or pending session."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> CancelSession(int sessionId, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _sessionMeetingService.CancelSessionAsync(userId, sessionId, ct);
                return Ok(new { message = "Session cancelled successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while cancelling the session.", details = ex.Message });
            }
        }

        /// <summary>
        /// Proposes a new schedule for an existing session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="dto">The new schedule details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">Session rescheduled successfully.</response>
        /// <response code="400">Invalid new schedule or session cannot be rescheduled.</response>
        /// <response code="401">User is not authenticated or not authorized to reschedule this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpPost("{sessionId}/reschedule")]
        [SwaggerOperation(
            Summary = "Reschedule a session",
            Description = "Proposes a new date and time for an existing session. The other party must accept the proposal."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> RescheduleSession(int sessionId, [FromBody] RescheduleSessionDTO dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _sessionMeetingService.RescheduleSessionAsync(userId, sessionId, dto.NewScheduledAt, dto.Comment, ct);
                return Ok(new { message = "Session rescheduled successfully. The other party must accept the proposal." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while rescheduling the session.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of sessions requested by the current user.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of requested sessions.</returns>
        /// <response code="200">Sessions retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("requested")]
        [SwaggerOperation(
            Summary = "Get requested sessions",
            Description = "Retrieves a list of all help sessions requested by the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> GetRequestedSessions(CancellationToken ct)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                var sessions = await _sessionMeetingService.GetRequestedSessionsAsync(requesterId, ct);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching requested sessions.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of session requests received by the current user.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of received sessions.</returns>
        /// <response code="200">Sessions retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("received")]
        [SwaggerOperation(
            Summary = "Get received sessions",
            Description = "Retrieves a list of all help session requests received by the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> GetReceivedSessions(CancellationToken ct)
        {
            try
            {
                var helperId = GetCurrentUserId();
                var sessions = await _sessionMeetingService.GetReceivedSessionsAsync(helperId, ct);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching received sessions.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves details of a specific session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The session details.</returns>
        /// <response code="200">Session retrieved successfully.</response>
        /// <response code="401">User is not authenticated or not authorized to view this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("{sessionId}")]
        [SwaggerOperation(
            Summary = "Get a session by ID",
            Description = "Retrieves detailed information about a specific session. User must be a participant."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> GetSessionById(int sessionId, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var session = await _sessionMeetingService.GetSessionByIdAsync(userId, sessionId, ct);
                return Ok(session);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching session details.", details = ex.Message });
            }
        }

        /// <summary>
        /// Generates a ZegoCloud access token for an active session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <returns>The ZegoCloud token and meeting details.</returns>
        /// <response code="200">Token generated successfully.</response>
        /// <response code="400">Session hasn't started, already ended, or not available to join.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to join this session.</response>
        /// <response code="404">Session not found.</response>
        /// <response code="500">Room not configured or an internal error occurred.</response>
        [HttpGet("{sessionId}/zego-token")]
        [SwaggerOperation(
            Summary = "Get a ZegoCloud token",
            Description = "Generates a real-time communication token for joining a session's meeting room."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<IActionResult> GetZegoToken(int sessionId)
        {
            var currentUserId = GetCurrentUserId();

            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId);
            if (session == null) return NotFound();

            // Gate 1 — only the 2 participants
            if (session.RequesterId != currentUserId && session.HelperId != currentUserId)
                return Forbid();

            // Gate 2 — only when meeting is active (or up to 2 min early)
            var now = DateTime.UtcNow;
            var scheduledAt = session.ScheduledAt.Kind == DateTimeKind.Utc
                ? session.ScheduledAt
                : DateTime.SpecifyKind(session.ScheduledAt, DateTimeKind.Utc);
            var durationMinutes = (int)session.DurationMinutes is 15 or 30 or 60
                ? (int)session.DurationMinutes
                : session.CreditCost;
            var endsAt = scheduledAt.AddMinutes(durationMinutes);

            if (now < scheduledAt.AddMinutes(-2))
                return BadRequest(new { message = "Session hasn't started yet." });

            if (now > endsAt)
                return BadRequest(new { message = "Session has already ended." });

            // Gate 3 — must be Accepted or Active
            if (session.Status != SessionStatus.Accepted && session.Status != SessionStatus.Active)
                return BadRequest(new { message = "Session is not available to join." });

            // Gate 4 — room must exist (was set on acceptance)
            if (string.IsNullOrEmpty(session.ZegoRoomId))
                return StatusCode(500, new { message = "Room not configured." });

            var userId = currentUserId.ToString();
            var userName = currentUserId == session.RequesterId
                ? session.Requester?.FullName ?? $"User {userId}"
                : session.Helper?.FullName ?? $"User {userId}";
            var token = _zego.GenerateToken(userId, endsAt);

            return Ok(new
            {
                token,
                roomId = session.ZegoRoomId,
                userId,
                userName,
                appId = _zego.AppId,
                endsAt
            });
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
