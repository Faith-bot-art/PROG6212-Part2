using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories for a specific event (public)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            var categories = await _categoryService.GetCategoriesByEventAsync(eventId);
            return Ok(categories);
        }

        /// <summary>
        /// Get category by ID (public)
        /// </summary>
        [HttpGet("{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
                return NotFound(new { error = "Category not found" });

            return Ok(category);
        }

        /// <summary>
        /// Create a new category for an event (Organiser - owner only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.CreateCategoryAsync(userId, dto);
                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.CategoryId }, category);
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
        /// Update a category (Organiser - owner only)
        /// </summary>
        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.UpdateCategoryAsync(userId, categoryId, dto);
                return Ok(category);
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
        /// Delete a category (Organiser - owner only)
        /// </summary>
        [HttpDelete("{categoryId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _categoryService.DeleteCategoryAsync(userId, categoryId);

                if (!deleted)
                    return NotFound(new { error = "Category not found" });

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