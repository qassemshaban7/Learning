using Learning_platform.DTO;
using Learning_platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Learning_platform.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("addcategory")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest("Invalid category data.");
            }
            if (_context.Category.Any(c => c.Name == categoryDTO.Name))
            {
                return BadRequest($"Category with name '{categoryDTO.Name}' already exists.");
            }

            var category = new Category
            {
                Name = categoryDTO.Name
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return Ok("Category added successfully.");
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("updatecategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO updatedCategoryDTO)
        {
            var existingCategory = await _context.Category.FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound("Category not found.");
            }

            if (_context.Category.Any(c => c.Name == updatedCategoryDTO.Name && c.Id != id))
            {
                return BadRequest($"Category with name '{updatedCategoryDTO.Name}' already exists.");
            }

            existingCategory.Name = updatedCategoryDTO.Name;

            await _context.SaveChangesAsync();

            return Ok("Category updated successfully.");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("deletecategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return Ok("Category deleted successfully.");
        }

        [HttpGet("getcategorybyId/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return Ok(category);
        }
        [HttpGet("getallcategory")]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Category.ToList();
            return Ok(categories);
        }
    }
}
