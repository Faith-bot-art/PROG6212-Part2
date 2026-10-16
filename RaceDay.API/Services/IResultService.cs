using RaceDay.API.DTOs;

namespace RaceDay.API.Services
{
    public interface IResultService
    {
        Task<IEnumerable<ResultDto>> GetResultsByEventAsync(int eventId);
        Task<IEnumerable<ResultDto>> GetMyResultsAsync(int userId);
        Task<ResultDto?> GetResultByIdAsync(int resultId);
        Task<ResultDto> CreateResultAsync(int userId, CreateResultDto dto);
        Task<ResultDto> UpdateResultAsync(int userId, int resultId, UpdateResultDto dto);
        Task<bool> DeleteResultAsync(int userId, int resultId);
    }
}