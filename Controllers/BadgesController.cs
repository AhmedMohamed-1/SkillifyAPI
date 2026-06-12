using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs.Badge.BadgeDTO;
using SkillifyAPI.Services.BadgeService;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling badges catalog retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgeService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgesController"/> class.
        /// </summary>
        /// <param name="service">The badge service instance.</param>
        public BadgesController(IBadgeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all badges in the system.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of badge DTOs.</returns>
        /// <response code="200">Badges list retrieved successfully.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all badges",
            Description = "Retrieves a list of all badges in the system."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetBadgeDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetBadgeDTO>>> GetAll(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching badges: " + ex.Message });
            }
        }
    }
}
