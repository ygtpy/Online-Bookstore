using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Data;
using BookStoreAPI.Models;
using System.Text.Json;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly BookStoreDbContext _context;

        public CategoriesController(BookStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    Books = c.Books.Where(b => b.IsActive).Select(b => new Book
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Author = b.Author,
                        Description = b.Description,
                        Price = b.Price,
                        ImageUrl = b.ImageUrl,
                        Stock = b.Stock,
                        IsActive = b.IsActive,
                        CreatedDate = b.CreatedDate,
                        CategoryId = b.CategoryId
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromBody] object categoryData)
        {
            try
            {
                // JSON'dan kategori verilerini al
                var jsonElement = (JsonElement)categoryData;

                var category = new Category
                {
                    Name = jsonElement.GetProperty("name").GetString(),
                    Description = jsonElement.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    ImageUrl = jsonElement.TryGetProperty("imageUrl", out var img) ? img.GetString() : null,
                    IsActive = jsonElement.TryGetProperty("isActive", out var active) ? active.GetBoolean() : true,
                    CreatedDate = DateTime.Now
                };

                // Validation
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return BadRequest("Category name is required.");
                }

                // Check if category name already exists
                var existingCategory = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
                if (existingCategory)
                {
                    return BadRequest("A category with this name already exists.");
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCategory", new { id = category.Id }, new Category
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    CreatedDate = category.CreatedDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating category: {ex.Message}");
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}