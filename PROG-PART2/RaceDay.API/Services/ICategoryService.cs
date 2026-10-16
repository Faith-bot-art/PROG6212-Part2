using RaceDay.API.DTOs;

namespace RaceDay.API.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesByEventAsync(int eventId);
        Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);
        Task<CategoryDto> CreateCategoryAsync(int userId, CreateCategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int userId, int categoryId);
    }
}