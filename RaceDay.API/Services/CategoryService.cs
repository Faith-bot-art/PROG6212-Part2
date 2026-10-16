using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesByEventAsync(int eventId)
        {
            var categories = await _context.Categories
                .Include(c => c.Event)
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.StartTime)
                .ToListAsync();

            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            return category == null ? null : MapToDto(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(int userId, CreateCategoryDto dto)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (ev == null)
                throw new InvalidOperationException("Event not found");

            if (ev.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only add categories to your own events");

            if (dto.MinAge.HasValue && dto.MaxAge.HasValue && dto.MinAge > dto.MaxAge)
                throw new InvalidOperationException("Minimum age cannot be greater than maximum age");

            var category = new Category
            {
                EventId = dto.EventId,
                Name = dto.Name,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                EntryFee = dto.EntryFee,
                StartTime = dto.StartTime,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(category.CategoryId)
                   ?? throw new InvalidOperationException("Failed to create category");
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found");

            if (category.Event?.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only update categories on your own events");

            if (dto.MinAge.HasValue && dto.MaxAge.HasValue && dto.MinAge > dto.MaxAge)
                throw new InvalidOperationException("Minimum age cannot be greater than maximum age");

            category.Name = dto.Name;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.EntryFee = dto.EntryFee;
            category.StartTime = dto.StartTime;

            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(categoryId)
                   ?? throw new InvalidOperationException("Failed to update category");
        }

        public async Task<bool> DeleteCategoryAsync(int userId, int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                return false;

            if (category.Event?.Organiser?.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete categories on your own events");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                EventName = c.Event?.Name ?? "Unknown",
                Name = c.Name,
                MinAge = c.MinAge,
                MaxAge = c.MaxAge,
                EntryFee = c.EntryFee,
                StartTime = c.StartTime,
                CreatedAt = c.CreatedAt
            };
        }
    }
}