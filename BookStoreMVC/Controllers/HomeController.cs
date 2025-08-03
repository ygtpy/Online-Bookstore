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
            // Ana sayfa i�in en son eklenen kitaplar� ve kategorileri getir
            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

            ViewBag.FeaturedBooks = books.Take(6).ToList();
            ViewBag.Categories = categories.Take(4).ToList();

            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Hakk�m�zda";
            ViewData["Message"] = "Kitap Ma�azam�z hakk�nda bilgiler";
            return View();
        }

        public IActionResult Contact()
        {
            TempData["ContactMessage"] = "Bizimle ileti�ime ge�mek i�in a�a��daki formu kullanabilirsiniz.";
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // Form verilerini i�le (ger�ek uygulamada e-posta g�nderimi yap�labilir)
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                TempData["SuccessMessage"] = "Mesaj�n�z ba�ar�yla g�nderildi. En k�sa s�rede d�n�� yapaca��z.";
                ViewData["FormSubmitted"] = true;
            }
            else
            {
                ViewData["ErrorMessage"] = "L�tfen t�m alanlar� doldurunuz.";
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