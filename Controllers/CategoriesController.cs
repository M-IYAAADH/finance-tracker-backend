using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Api.Data;
using System.Security.Claims;
using FinanceTracker.Api.Domain;
using FinanceTracker.Api.DTOs.Categories;


namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                            ?? User.FindFirstValue("sub");

            return Guid.Parse(userId!);             
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var userId = GetUserId();

            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(categories);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var userId = GetUserId();

            if (string .IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            if (!Enum.IsDefined(typeof(CategoryType), dto.Type))
            {
                return BadRequest("Invalid category type.");
            }

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == dto.Name.ToLower());

            if (existingCategory != null)
            {
                return Conflict("A category with the same name already exists.");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Type = (CategoryType)dto.Type,
                UserId = userId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryDto dto)
        {
            var userId = GetUserId();

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Category name is required.");

            if (!Enum.IsDefined(typeof(CategoryType), dto.Type))
                return BadRequest("Invalid category type.");

            // Fetch category owned by user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return NotFound("Category not found.");

            // Prevent duplicate name
            var exists = await _context.Categories.AnyAsync(c =>
                c.UserId == userId &&
                c.Name.ToLower() == dto.Name.ToLower() &&
                c.Id != id);

            if (exists)
                return BadRequest("Category already exists.");

            // Update
            category.Name = dto.Name;
            category.Type = (CategoryType)dto.Type;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var userId = GetUserId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
                
        }

    }
}
