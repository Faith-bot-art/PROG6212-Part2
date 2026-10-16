using RaceDay.API.DTOs;

namespace RaceDay.API.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventDto>> GetAllEventsAsync(string? eventType, string? location, bool? isActive);
        Task<EventDto?> GetEventByIdAsync(int eventId);
        Task<EventDto> CreateEventAsync(int userId, CreateEventDto dto);
        Task<EventDto> UpdateEventAsync(int userId, int eventId, UpdateEventDto dto);
        Task<bool> DeleteEventAsync(int userId, int eventId);
        Task<IEnumerable<EventDto>> GetMyEventsAsync(int userId);
    }
}