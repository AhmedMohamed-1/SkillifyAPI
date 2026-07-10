using Microsoft.AspNetCore.Mvc;
using SkillifyAPI.DTOs;
using SkillifyAPI.DTOs.CreditTransaction;
using SkillifyAPI.Services.CreditTransactionService;
using System.Security.Claims;

namespace SkillifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditTransactionsController : ControllerBase
    {
        private readonly ICreditTransactionService _service;

        public CreditTransactionsController(ICreditTransactionService service)
        {
            _service = service;
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreditTransactionHistoryDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var result = await _service.GetUserHistoryAsync(GetCurrentUserId());
                return Ok(result);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching credits history." + ex.Message });
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
