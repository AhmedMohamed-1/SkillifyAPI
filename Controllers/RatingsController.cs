using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs.Rating;
using SkillifyAPI.Services.RatingService;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace SkillifyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Submit a session rating",
            Description = "Submits a rating and optional review text for a completed session. One rating is allowed per session."
        )]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GetRatingDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitRating([FromBody] SubmitRatingDTO dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var rating = await _ratingService.SubmitRatingAsync(userId, dto, ct);
                return CreatedAtAction(nameof(GetBySessionId), new { sessionId = rating.SessionId }, rating);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Errors.First().ErrorMessage });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while submitting the rating.", details = ex.Message });
            }
        }

        [HttpGet("received")]
        [SwaggerOperation(
            Summary = "Get my received reviews",
            Description = "Retrieves all reviews received by the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetReceivedReviewDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyReceivedReviews(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reviews = await _ratingService.GetMyReceivedReviewsAsync(userId, ct);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching received reviews.", details = ex.Message });
            }
        }

        [HttpGet("given")]
        [SwaggerOperation(
            Summary = "Get my given reviews",
            Description = "Retrieves all reviews submitted by the currently authenticated user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetRatingDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyGivenReviews(CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var reviews = await _ratingService.GetMyGivenReviewsAsync(userId, ct);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching given reviews.", details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        [SwaggerOperation(
            Summary = "Get a user's received reviews",
            Description = "Retrieves all public reviews received by the specified user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetReceivedReviewDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserReceivedReviews(int userId, CancellationToken ct)
        {
            try
            {
                var reviews = await _ratingService.GetUserReceivedReviewsAsync(userId, ct);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching user reviews.", details = ex.Message });
            }
        }

        [HttpGet("session/{sessionId}")]
        [SwaggerOperation(
            Summary = "Get rating for a session",
            Description = "Retrieves the rating for a session if one exists. Only session participants can access it."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRatingDTO))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySessionId(int sessionId, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var rating = await _ratingService.GetBySessionIdAsync(userId, sessionId, ct);
                if (rating == null)
                    return NoContent();

                return Ok(rating);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching the session rating.", details = ex.Message });
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
