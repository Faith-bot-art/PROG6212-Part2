using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly IEnrolmentService _enrolmentService;

        public EnrolmentsController(IEnrolmentService enrolmentService)
        {
            _enrolmentService = enrolmentService;
        }

        /// <summary>
        /// Get all enrolments for the current participant
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetCurrentUserId();
            var enrolments = await _enrolmentService.GetMyEnrolmentsAsync(userId);
            return Ok(enrolments);
        }

        /// <summary>
        /// Get all enrolments for a specific event (Organiser - owner only)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsByEvent(int eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var enrolments = await _enrolmentService.GetEnrolmentsByEventAsync(userId, eventId);
                return Ok(enrolments);
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
        /// Get enrolment by ID
        /// </summary>
        [HttpGet("{enrolmentId}")]
        public async Task<IActionResult> GetEnrolmentById(int enrolmentId)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(enrolmentId);
            if (enrolment == null)
                return NotFound(new { error = "Enrolment not found" });

            return Ok(enrolment);
        }

        /// <summary>
        /// Enrol in an event (Participant only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment([FromBody] CreateEnrolmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var enrolment = await _enrolmentService.CreateEnrolmentAsync(userId, dto);
                return CreatedAtAction(nameof(GetEnrolmentById), new { enrolmentId = enrolment.EnrolmentId }, enrolment);
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
        /// Update an enrolment (status: Registered / Confirmed / Cancelled)
        /// </summary>
        [HttpPut("{enrolmentId}")]
        public async Task<IActionResult> UpdateEnrolment(int enrolmentId, [FromBody] UpdateEnrolmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var enrolment = await _enrolmentService.UpdateEnrolmentAsync(userId, enrolmentId, dto);
                return Ok(enrolment);
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
        /// Cancel an enrolment
        /// </summary>
        [HttpDelete("{enrolmentId}")]
        public async Task<IActionResult> DeleteEnrolment(int enrolmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _enrolmentService.DeleteEnrolmentAsync(userId, enrolmentId);

                if (!deleted)
                    return NotFound(new { error = "Enrolment not found" });

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