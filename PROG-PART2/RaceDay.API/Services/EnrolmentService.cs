using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class EnrolmentService : IEnrolmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EnrolmentDto>> GetMyEnrolmentsAsync(int userId)
        {
            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return Enumerable.Empty<EnrolmentDto>();

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p!.User)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == participant.ProfileId)
                .OrderByDescending(e => e.RegistrationDate)
                .ToListAsync();

            return enrolments.Select(MapToDto);
        }

        public async Task<IEnumerable<EnrolmentDto>> GetEnrolmentsByEventAsync(int userId, int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                throw new InvalidOperationException("Event not found");

            if (ev.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only view enrolments for your own events");

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p!.User)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.EventId == eventId)
                .OrderBy(e => e.BibNumber)
                .ToListAsync();

            return enrolments.Select(MapToDto);
        }

        public async Task<EnrolmentDto?> GetEnrolmentByIdAsync(int enrolmentId)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p!.User)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            return enrolment == null ? null : MapToDto(enrolment);
        }

        public async Task<EnrolmentDto> CreateEnrolmentAsync(int userId, CreateEnrolmentDto dto)
        {
            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                throw new UnauthorizedAccessException("Only participants can enrol in events");

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == dto.EventId);
            if (ev == null)
                throw new InvalidOperationException("Event not found");

            if (!ev.IsActive)
                throw new InvalidOperationException("Event is not active");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found");

            if (category.EventId != dto.EventId)
                throw new InvalidOperationException("Category does not belong to this event");

            // Check if already enrolled in this event
            var existing = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participant.ProfileId
                                       && e.EventId == dto.EventId
                                       && e.Status != "Cancelled");

            if (existing != null)
                throw new InvalidOperationException("You are already enrolled in this event");

            // Generate a unique bib number
            var maxBib = await _context.Enrolments
                .Where(e => e.BibNumber != null)
                .Select(e => e.BibNumber)
                .MaxAsync();

            var bibNumber = (maxBib ?? 100) + 1;

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ProfileId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                RegistrationDate = DateTime.UtcNow,
                Status = "Registered",
                BibNumber = bibNumber
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return await GetEnrolmentByIdAsync(enrolment.EnrolmentId)
                   ?? throw new InvalidOperationException("Failed to create enrolment");
        }

        public async Task<EnrolmentDto> UpdateEnrolmentAsync(int userId, int enrolmentId, UpdateEnrolmentDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event).ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null)
                throw new InvalidOperationException("Enrolment not found");

            bool isOwnerParticipant = enrolment.Participant?.UserId == userId;
            bool isEventOrganiser = enrolment.Event?.Organiser?.UserId == userId;

            if (!isOwnerParticipant && !isEventOrganiser)
                throw new UnauthorizedAccessException("You cannot modify this enrolment");

            if (dto.Status != "Registered" && dto.Status != "Confirmed" && dto.Status != "Cancelled")
                throw new InvalidOperationException("Status must be Registered, Confirmed, or Cancelled");

            enrolment.Status = dto.Status;
            await _context.SaveChangesAsync();

            return await GetEnrolmentByIdAsync(enrolmentId)
                   ?? throw new InvalidOperationException("Failed to update enrolment");
        }

        public async Task<bool> DeleteEnrolmentAsync(int userId, int enrolmentId)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event).ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null)
                return false;

            bool isOwnerParticipant = enrolment.Participant?.UserId == userId;
            bool isEventOrganiser = enrolment.Event?.Organiser?.UserId == userId;

            if (!isOwnerParticipant && !isEventOrganiser)
                throw new UnauthorizedAccessException("You cannot cancel this enrolment");

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return true;
        }

        private static EnrolmentDto MapToDto(Enrolment e)
        {
            return new EnrolmentDto
            {
                EnrolmentId = e.EnrolmentId,
                ParticipantId = e.ParticipantId,
                ParticipantName = e.Participant?.User?.FullName ?? "Unknown",
                EventId = e.EventId,
                EventName = e.Event?.Name ?? "Unknown",
                EventStartDate = e.Event?.StartDate ?? DateTime.MinValue,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name ?? "Unknown",
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                BibNumber = e.BibNumber,
                HasResult = e.Result != null
            };
        }
    }
}