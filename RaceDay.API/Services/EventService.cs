using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(string? eventType, string? location, bool? isActive)
        {
            var query = _context.Events
                .Include(e => e.Organiser)
                    .ThenInclude(o => o!.User)
                .Include(e => e.Categories)
                .AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(e => e.Location.Contains(location));

            if (isActive.HasValue)
                query = query.Where(e => e.IsActive == isActive.Value);

            var events = await query.OrderBy(e => e.StartDate).ToListAsync();
            return events.Select(MapToDto);
        }

        public async Task<EventDto?> GetEventByIdAsync(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                    .ThenInclude(o => o!.User)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            return ev == null ? null : MapToDto(ev);
        }

        public async Task<EventDto> CreateEventAsync(int userId, CreateEventDto dto)
        {
            var organiser = await _context.OrganiserProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                throw new UnauthorizedAccessException("Only organisers can create events");

            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("End date must be after start date");

            if (dto.EventType != "Running" && dto.EventType != "Walking" && dto.EventType != "Cycling")
                throw new InvalidOperationException("Event type must be Running, Walking, or Cycling");

            var ev = new Event
            {
                OrganiserId = organiser.ProfileId,
                Name = dto.Name,
                Description = dto.Description,
                EventType = dto.EventType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(ev.EventId) ?? throw new InvalidOperationException("Failed to create event");
        }

        public async Task<EventDto> UpdateEventAsync(int userId, int eventId, UpdateEventDto dto)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                throw new InvalidOperationException("Event not found");

            if (ev.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own events");

            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("End date must be after start date");

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.Location = dto.Location;
            ev.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(eventId) ?? throw new InvalidOperationException("Failed to update event");
        }

        public async Task<bool> DeleteEventAsync(int userId, int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                return false;

            if (ev.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own events");

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EventDto>> GetMyEventsAsync(int userId)
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                    .ThenInclude(o => o!.User)
                .Include(e => e.Categories)
                .Where(e => e.Organiser!.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return events.Select(MapToDto);
        }

        private static EventDto MapToDto(Event ev)
        {
            return new EventDto
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = ev.Organiser?.OrganisationName ?? "Unknown",
                Name = ev.Name,
                Description = ev.Description,
                EventType = ev.EventType,
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                Location = ev.Location,
                DistanceKm = ev.DistanceKm,
                IsActive = ev.IsActive,
                CreatedAt = ev.CreatedAt,
                CategoryCount = ev.Categories?.Count ?? 0
            };
        }
    }
}