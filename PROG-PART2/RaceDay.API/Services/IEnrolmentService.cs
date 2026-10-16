using RaceDay.API.DTOs;

namespace RaceDay.API.Services
{
    public interface IEnrolmentService
    {
        Task<IEnumerable<EnrolmentDto>> GetMyEnrolmentsAsync(int userId);
        Task<IEnumerable<EnrolmentDto>> GetEnrolmentsByEventAsync(int userId, int eventId);
        Task<EnrolmentDto?> GetEnrolmentByIdAsync(int enrolmentId);
        Task<EnrolmentDto> CreateEnrolmentAsync(int userId, CreateEnrolmentDto dto);
        Task<EnrolmentDto> UpdateEnrolmentAsync(int userId, int enrolmentId, UpdateEnrolmentDto dto);
        Task<bool> DeleteEnrolmentAsync(int userId, int enrolmentId);
    }
}