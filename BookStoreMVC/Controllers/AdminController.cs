using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApiService apiService, IWebHostEnvironment environment)
        {
            _apiService = apiService;
            _environment = environment;
        }

        // Admin Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

            ViewBag.TotalBooks = books.Count;
            ViewBag.TotalCategories = categories.Count;
            ViewBag.RecentBooks = books.OrderByDescending(b => b.CreatedDate).Take(5).ToList();

            return View();
        }

        // Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "admin@bookstore.com" && password == "admin123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminEmail", email);
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Geçersiz email veya şifre.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #region Books Management

        public async Task<IActionResult> Books()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

            ViewBag.Categories = categories;
            return View(books);
        }

        public async Task<IActionResult> CreateBook()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var categories = await _apiService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(BookDto book, IFormFile? imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    book.ImageUrl = await SaveImageAsync(imageFile, "books");
                }

                // Create clean book object
                var cleanBook = new BookDto
                {
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl,
                    Stock = book.Stock,
                    IsActive = book.IsActive,
                    CategoryId = book.CategoryId
                };

                var result = await _apiService.CreateBookAsync(cleanBook);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Kitap başarıyla eklendi.";
                    return RedirectToAction("Books");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap eklenirken bir hata oluştu: {ex.Message}";
            }

            var categories = await _apiService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.ErrorMessage = "Kitap eklenirken bir hata oluştu.";
            return View(book);
        }

        public async Task<IActionResult> EditBook(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var book = await _apiService.GetBookAsync(id);
            if (book == null) return NotFound();

            var categories = await _apiService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(int id, BookDto book, IFormFile? imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            if (id != book.Id) return BadRequest();

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(book.ImageUrl))
                    {
                        DeleteOldImage(book.ImageUrl);
                    }

                    book.ImageUrl = await SaveImageAsync(imageFile, "books");
                }

                // Create clean book object
                var cleanBook = new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl,
                    Stock = book.Stock,
                    IsActive = book.IsActive,
                    CategoryId = book.CategoryId,
                    CreatedDate = book.CreatedDate
                };

                var success = await _apiService.UpdateBookAsync(id, cleanBook);
                if (success)
                {
                    TempData["SuccessMessage"] = "Kitap başarıyla güncellendi.";
                    return RedirectToAction("Books");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kitap güncellenirken bir hata oluştu: {ex.Message}";
            }

            var categories = await _apiService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.ErrorMessage = "Kitap güncellenirken bir hata oluştu.";
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Yetki yok." });

            var success = await _apiService.DeleteBookAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Kitap başarıyla silindi." });
            }

            return Json(new { success = false, message = "Kitap silinirken bir hata oluştu." });
        }

        #endregion

        #region Categories Management

        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var categories = await _apiService.GetCategoriesAsync();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDto category, IFormFile? imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            try
            {
                // Debug: Log the incoming data
                Console.WriteLine($"Creating category: {category.Name}");
                Console.WriteLine($"Description: {category.Description}");
                Console.WriteLine($"IsActive: {category.IsActive}");
                Console.WriteLine($"Image file: {imageFile?.FileName ?? "No file"}");

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    category.ImageUrl = await SaveImageAsync(imageFile, "categories");
                    Console.WriteLine($"Image saved: {category.ImageUrl}");
                }

                // Create clean category object
                var cleanCategory = new CategoryDto
                {
                    Name = category.Name?.Trim(),
                    Description = category.Description?.Trim(),
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive
                };

                // Validate required fields
                if (string.IsNullOrWhiteSpace(cleanCategory.Name))
                {
                    ViewBag.ErrorMessage = "Kategori adı zorunludur.";
                    return View(category);
                }

                Console.WriteLine("Calling API to create category...");
                var result = await _apiService.CreateCategoryAsync(cleanCategory);

                if (result != null)
                {
                    Console.WriteLine($"Category created successfully with ID: {result.Id}");
                    TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                    return RedirectToAction("Categories");
                }
                else
                {
                    Console.WriteLine("API returned null result");
                    ViewBag.ErrorMessage = "API'den geçersiz yanıt alındı.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateCategory: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Kategori eklenirken bir hata oluştu: {ex.Message}";
            }

            ViewBag.ErrorMessage = "Kategori eklenirken bir hata oluştu.";
            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var category = await _apiService.GetCategoryAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, CategoryDto category, IFormFile? imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            if (id != category.Id) return BadRequest();

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        DeleteOldImage(category.ImageUrl);
                    }

                    category.ImageUrl = await SaveImageAsync(imageFile, "categories");
                }

                var success = await _apiService.UpdateCategoryAsync(id, category);
                if (success)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                    return RedirectToAction("Categories");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kategori güncellenirken bir hata oluştu: {ex.Message}";
            }

            ViewBag.ErrorMessage = "Kategori güncellenirken bir hata oluştu.";
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Yetki yok." });

            var success = await _apiService.DeleteCategoryAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Kategori başarıyla silindi." });
            }

            return Json(new { success = false, message = "Kategori silinirken bir hata oluştu." });
        }

        #endregion

        #region Orders Management

        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            return View();
        }

        #endregion

        #region Helper Methods

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile, string folder)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }

        private void DeleteOldImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("/images/"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
        }

        #endregion
    }
}