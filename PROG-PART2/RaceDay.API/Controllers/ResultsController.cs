using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IResultService _resultService;

        public ResultsController(IResultService resultService)
        {
            _resultService = resultService;
        }

        /// <summary>
        /// Get all results for a specific event (public)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResultsByEvent(int eventId)
        {
            var results = await _resultService.GetResultsByEventAsync(eventId);
            return Ok(results);
        }

        /// <summary>
        /// Get all results for the current participant
        /// </summary>
        [HttpGet("my-results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();
            var results = await _resultService.GetMyResultsAsync(userId);
            return Ok(results);
        }

        /// <summary>
        /// Get result by ID (public)
        /// </summary>
        [HttpGet("{resultId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResultById(int resultId)
        {
            var result = await _resultService.GetResultByIdAsync(resultId);
            if (result == null)
                return NotFound(new { error = "Result not found" });

            return Ok(result);
        }

        /// <summary>
        /// Upload a result for an enrolment (Organiser - owner only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _resultService.CreateResultAsync(userId, dto);
                return CreatedAtAction(nameof(GetResultById), new { resultId = result.ResultId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a result (Organiser - owner only)
        /// </summary>
        [HttpPut("{resultId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int resultId, [FromBody] UpdateResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var result = await _resultService.UpdateResultAsync(userId, resultId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a result (Organiser - owner only)
        /// </summary>
        [HttpDelete("{resultId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int resultId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _resultService.DeleteResultAsync(userId, resultId);

                if (!deleted)
                    return NotFound(new { error = "Result not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(userIdClaim);
        }
    }
}