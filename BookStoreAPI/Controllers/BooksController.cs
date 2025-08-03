using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Data;
using BookStoreAPI.Models;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookStoreDbContext _context;

        public BooksController(BookStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .Select(b => new Book
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
                    CategoryId = b.CategoryId,
                    Category = new Category
                    {
                        Id = b.Category.Id,
                        Name = b.Category.Name,
                        Description = b.Category.Description,
                        ImageUrl = b.Category.ImageUrl,
                        IsActive = b.Category.IsActive,
                        CreatedDate = b.Category.CreatedDate
                    }
                })
                .ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Id == id)
                .Select(b => new Book
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
                    CategoryId = b.CategoryId,
                    Category = new Category
                    {
                        Id = b.Category.Id,
                        Name = b.Category.Name,
                        Description = b.Category.Description,
                        ImageUrl = b.Category.ImageUrl,
                        IsActive = b.Category.IsActive,
                        CreatedDate = b.Category.CreatedDate
                    }
                })
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // GET: api/Books/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByCategory(int categoryId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId && b.IsActive)
                .Select(b => new Book
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
                    CategoryId = b.CategoryId,
                    Category = new Category
                    {
                        Id = b.Category.Id,
                        Name = b.Category.Name,
                        Description = b.Category.Description,
                        ImageUrl = b.Category.ImageUrl,
                        IsActive = b.Category.IsActive,
                        CreatedDate = b.Category.CreatedDate
                    }
                })
                .ToListAsync();
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
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

        // POST: api/Books
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            try
            {
                // Category navigation property'sini temizle
                book.Category = null;

                // Validation
                if (string.IsNullOrWhiteSpace(book.Title))
                {
                    return BadRequest("Title is required.");
                }

                if (string.IsNullOrWhiteSpace(book.Author))
                {
                    return BadRequest("Author is required.");
                }

                if (book.CategoryId <= 0)
                {
                    return BadRequest("Valid CategoryId is required.");
                }

                if (book.Price <= 0)
                {
                    return BadRequest("Price must be greater than 0.");
                }

                // Check if category exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == book.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest("Invalid CategoryId. Category does not exist.");
                }

                // Set defaults
                book.CreatedDate = DateTime.Now;
                book.IsActive = book.IsActive; // Use provided value or default

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetBook", new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating book: {ex.Message}");
            }
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            book.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}