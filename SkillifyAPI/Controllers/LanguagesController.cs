using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs;
using SkillifyAPI.DTOs.Language.LanguageDTO;
using SkillifyAPI.Services.LanguageService;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace SkillifyAPI.Controllers
{
    /// <summary>
    /// Controller handling languages catalog administration and retrieval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LanguagesController : ControllerBase
    {
        private readonly ILanguageService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguagesController"/> class.
        /// </summary>
        /// <param name="service">The language service instance.</param>
        public LanguagesController(ILanguageService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves a list of all languages in alphabetical order.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A collection of language DTOs.</returns>
        /// <response code="200">Languages list retrieved successfully.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all languages",
            Description = "Retrieves a list of all languages in alphabetical order."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetLanguageDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<IEnumerable<GetLanguageDTO>>> GetAll(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching languages: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific language by its unique integer identifier.
        /// </summary>
        /// <param name="id">The language unique ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested language DTO.</returns>
        /// <response code="200">Language retrieved successfully.</response>
        /// <response code="404">Language with the specified ID was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Get a language by ID",
            Description = "Retrieves details of a specific language by its unique integer identifier."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetLanguageDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetLanguageDTO>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return NotFound(new { message = $"Language with ID {id} not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching language: " + ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific language by its unique standard ISO short code.
        /// </summary>
        /// <param name="code">The language short code (e.g. 'en', 'ar').</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The requested language DTO.</returns>
        /// <response code="200">Language retrieved successfully.</response>
        /// <response code="404">Language with the specified code was not found.</response>
        /// <response code="500">An unexpected internal server error occurred.</response>
        [HttpGet("code/{code}")]
        [SwaggerOperation(
            Summary = "Get a language by ISO code",
            Description = "Retrieves a specific language by its short code (e.g. 'en', 'ar'). Case-insensitive."
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetLanguageDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        public async Task<ActionResult<GetLanguageDTO>> GetByCode(string code, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByCodeAsync(code, ct);
                if (result == null)
                    return NotFound(new { message = $"Language with code '{code}' not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching language by code: " + ex.Message });
            }
        }
    }
}
