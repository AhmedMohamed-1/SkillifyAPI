using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.Services.SubSkillService;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling sub-skills catalog administration and retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SubSkillsController : ControllerBase
    {
        private readonly ISubSkillService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubSkillsController"/> class.
        /// </summary>
        /// <param name="service">The sub-skill service instance.</param>
        public SubSkillsController(ISubSkillService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all sub-skills in alphabetical order.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of sub-skill DTOs.</returns>
        /// <response code="200">Sub-skills list retrieved successfully.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all sub-skills",
            Description = "Retrieves a flat list of all sub-skills in alphabetical order."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetSubSkillDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetSubSkillDTO>>> GetAll(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching sub-skills: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all sub-skills associated with a specific main skill.
        /// </summary>
        /// <param name="mainSkillId">The unique ID of the parent main skill.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of sub-skills associated with the main skill.</returns>
        /// <response code="200">Sub-skills list retrieved successfully.</response>
        /// <response code="404">Main skill with the specified ID was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("mainskill/{mainSkillId:int}")]
        [SwaggerOperation(
            Summary = "Get sub-skills by main skill ID",
            Description = "Retrieves all sub-skills associated with a specific main skill identifier."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetSubSkillDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetSubSkillDTO>>> GetByMainSkillId(int mainSkillId, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByMainSkillIdAsync(mainSkillId, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching sub-skills for main skill: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific sub-skill by its unique integer identifier.
        /// </summary>
        /// <param name="id">The sub-skill unique ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested sub-skill DTO.</returns>
        /// <response code="200">Sub-skill retrieved successfully.</response>
        /// <response code="404">Sub-skill with the specified ID was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Get a sub-skill by ID",
            Description = "Retrieves details of a specific sub-skill by its unique integer identifier."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSubSkillDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetSubSkillDTO>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(new { message = $"Sub-skill with ID {id} not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching sub-skill: " + ex.Message });
            }
        }

    }
}
