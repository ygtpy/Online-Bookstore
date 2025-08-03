using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApiService _apiService;

        public CategoriesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _apiService.GetCategoriesAsync();

            ViewData["Title"] = "Kategoriler";
            ViewBag.Message = "Tüm kitap kategorilerimizi keşfedin";

            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _apiService.GetCategoryAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kategoriye ait kitapları getir
            var books = await _apiService.GetBooksByCategoryAsync(id);

            ViewBag.Books = books;
            ViewData["Title"] = category.Name;
            TempData["CategoryInfo"] = $"{category.Name} kategorisinde {books.Count} kitap bulunmaktadır.";

            return View(category);
        }
    }
}