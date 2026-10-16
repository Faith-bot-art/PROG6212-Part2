using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class ResultService : IResultService
    {
        private readonly ApplicationDbContext _context;

        public ResultService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ResultDto>> GetResultsByEventAsync(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.Position ?? int.MaxValue)
                .ToListAsync();

            return results.Select(MapToDto);
        }

        public async Task<IEnumerable<ResultDto>> GetMyResultsAsync(int userId)
        {
            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return Enumerable.Empty<ResultDto>();

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.ParticipantId == participant.ProfileId)
                .OrderByDescending(r => r.Enrolment!.Event!.StartDate)
                .ToListAsync();

            return results.Select(MapToDto);
        }

        public async Task<ResultDto?> GetResultByIdAsync(int resultId)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .FirstOrDefaultAsync(r => r.ResultId == resultId);

            return result == null ? null : MapToDto(result);
        }

        public async Task<ResultDto> CreateResultAsync(int userId, CreateResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Event).ThenInclude(ev => ev!.Organiser)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
                throw new InvalidOperationException("Enrolment not found");

            if (enrolment.Event?.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only upload results for your own events");

            if (enrolment.Result != null)
                throw new InvalidOperationException("A result already exists for this enrolment");

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Pace = dto.Pace,
                IsDisqualified = dto.IsDisqualified,
                CreatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return await GetResultByIdAsync(result.ResultId)
                   ?? throw new InvalidOperationException("Failed to create result");
        }

        public async Task<ResultDto> UpdateResultAsync(int userId, int resultId, UpdateResultDto dto)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event).ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(r => r.ResultId == resultId);

            if (result == null)
                throw new InvalidOperationException("Result not found");

            if (result.Enrolment?.Event?.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only update results for your own events");

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            result.Pace = dto.Pace;
            result.IsDisqualified = dto.IsDisqualified;

            await _context.SaveChangesAsync();

            return await GetResultByIdAsync(resultId)
                   ?? throw new InvalidOperationException("Failed to update result");
        }

        public async Task<bool> DeleteResultAsync(int userId, int resultId)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event).ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(r => r.ResultId == resultId);

            if (result == null)
                return false;

            if (result.Enrolment?.Event?.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete results for your own events");

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ResultDto MapToDto(Result r)
        {
            return new ResultDto
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                ParticipantId = r.Enrolment?.ParticipantId ?? 0,
                ParticipantName = r.Enrolment?.Participant?.User?.FullName ?? "Unknown",
                EventId = r.Enrolment?.EventId ?? 0,
                EventName = r.Enrolment?.Event?.Name ?? "Unknown",
                CategoryName = r.Enrolment?.Category?.Name ?? "Unknown",
                FinishTime = r.FinishTime,
                Position = r.Position,
                Pace = r.Pace,
                IsDisqualified = r.IsDisqualified,
                CreatedAt = r.CreatedAt
            };
        }
    }
}