using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Get all events with optional filters (public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents(
            [FromQuery] string? eventType,
            [FromQuery] string? location,
            [FromQuery] bool? isActive)
        {
            var events = await _eventService.GetAllEventsAsync(eventType, location, isActive);
            return Ok(events);
        }

        /// <summary>
        /// Get event by ID (public)
        /// </summary>
        [HttpGet("{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int eventId)
        {
            var ev = await _eventService.GetEventByIdAsync(eventId);
            if (ev == null)
                return NotFound(new { error = "Event not found" });

            return Ok(ev);
        }

        /// <summary>
        /// Create a new event (Organiser only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var ev = await _eventService.CreateEventAsync(userId, dto);
                return CreatedAtAction(nameof(GetEventById), new { eventId = ev.EventId }, ev);
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
        /// Update an existing event (Organiser - owner only)
        /// </summary>
        [HttpPut("{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int eventId, [FromBody] UpdateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var ev = await _eventService.UpdateEventAsync(userId, eventId, dto);
                return Ok(ev);
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
        /// Delete an event (Organiser - owner only)
        /// </summary>
        [HttpDelete("{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _eventService.DeleteEventAsync(userId, eventId);

                if (!deleted)
                    return NotFound(new { error = "Event not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get events created by the current organiser
        /// </summary>
        [HttpGet("my-events")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetCurrentUserId();
            var events = await _eventService.GetMyEventsAsync(userId);
            return Ok(events);
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