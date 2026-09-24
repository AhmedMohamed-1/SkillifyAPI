using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs;
using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.Services.MainSkillService;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling main skills catalog retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MainSkillsController : ControllerBase
    {
        private readonly IMainSkillService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainSkillsController"/> class.
        /// </summary>
        /// <param name="service">The main skill service instance.</param>
        public MainSkillsController(IMainSkillService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all main skills in alphabetical order.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of main skill DTOs.</returns>
        /// <response code="200">Main skills list retrieved successfully.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all main skills",
            Description = "Retrieves a flat list of all main skills in alphabetical order without child sub-skills."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetMainSkillDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetMainSkillDTO>>> GetAll(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching main skills: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of all main skills along with their associated sub-skills.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of main skill DTOs including sub-skills.</returns>
        /// <response code="200">Main skills with sub-skills retrieved successfully.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("with-subskills")]
        [SwaggerOperation(
            Summary = "Get all main skills with their sub-skills",
            Description = "Retrieves a rich hierarchical list of all main skills along with their associated sub-skills."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetMainSkillWithSubSkillsDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetMainSkillWithSubSkillsDTO>>> GetAllWithSubSkills(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllWithSubSkillsAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching main skills with sub-skills: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific main skill and its sub-skills by its unique integer identifier.
        /// </summary>
        /// <param name="id">The main skill unique ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested main skill DTO with sub-skills.</returns>
        /// <response code="200">Main skill retrieved successfully.</response>
        /// <response code="404">Main skill with the specified ID was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Get a main skill by ID",
            Description = "Retrieves details of a specific main skill including its sub-skills by its unique integer identifier."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMainSkillWithSubSkillsDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetMainSkillWithSubSkillsDTO>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(new { message = $"Main skill with ID {id} not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching main skill: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific main skill and its sub-skills by its unique slug.
        /// </summary>
        /// <param name="slug">The main skill slug (e.g. 'software-engineering').</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested main skill DTO with sub-skills.</returns>
        /// <response code="200">Main skill retrieved successfully.</response>
        /// <response code="404">Main skill with the specified slug was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("slug/{slug}")]
        [SwaggerOperation(
            Summary = "Get a main skill by slug",
            Description = "Retrieves details of a specific main skill including its sub-skills by its unique URL slug."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMainSkillWithSubSkillsDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetMainSkillWithSubSkillsDTO>> GetBySlug(string slug, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetBySlugAsync(slug, ct);
                if (result == null)
                    return NotFound(new { message = $"Main skill with slug '{slug}' not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching main skill by slug: " + ex.Message });
            }
        }
    }
}
