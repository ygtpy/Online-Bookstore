using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models;
using BookStoreMVC.Services;
using System.Diagnostics;

namespace BookStoreMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApiService apiService, ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Ana sayfa için en son eklenen kitaplarý ve kategorileri getir
            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

            ViewBag.FeaturedBooks = books.Take(6).ToList();
            ViewBag.Categories = categories.Take(4).ToList();

            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Hakkýmýzda";
            ViewData["Message"] = "Kitap Maðazamýz hakkýnda bilgiler";
            return View();
        }

        public IActionResult Contact()
        {
            TempData["ContactMessage"] = "Bizimle iletiþime geçmek için aþaðýdaki formu kullanabilirsiniz.";
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // Form verilerini iþle (gerçek uygulamada e-posta gönderimi yapýlabilir)
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Mesajýnýz baþarýyla gönderildi. En kýsa sürede dönüþ yapacaðýz.";
                ViewData["FormSubmitted"] = true;
            }
            else
            {
                ViewData["ErrorMessage"] = "Lütfen tüm alanlarý doldurunuz.";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}