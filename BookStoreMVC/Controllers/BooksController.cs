using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models;
using BookStoreMVC.Services;
using System.Text.Json;
using System.Text;

namespace BookStoreMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApiService _apiService;

        public BooksController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice)
        {
            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

            // Filtreleme
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
                ViewBag.SelectedCategoryId = categoryId.Value;
            }

            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                       b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.SearchTerm = search;
            }

            if (minPrice.HasValue)
            {
                books = books.Where(b => b.Price >= minPrice.Value).ToList();
                ViewBag.MinPrice = minPrice.Value;
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.Price <= maxPrice.Value).ToList();
                ViewBag.MaxPrice = maxPrice.Value;
            }

            ViewBag.Categories = categories;
            ViewData["Title"] = "Kitaplar";

            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _apiService.GetBookAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            // İlgili kitapları getir (aynı kategoriden)
            var relatedBooks = await _apiService.GetBooksByCategoryAsync(book.CategoryId);
            relatedBooks = relatedBooks.Where(b => b.Id != id).Take(4).ToList();

            ViewBag.RelatedBooks = relatedBooks;
            ViewData["Title"] = book.Title;
            TempData["BookDetails"] = $"{book.Title} - {book.Author}";

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            var book = await _apiService.GetBookAsync(bookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Kitap bulunamadı." });
            }

            // Session'dan sepeti al
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

            cart.AddItem(book, quantity);

            // Sepeti session'a kaydet
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            ViewBag.CartItemCount = cart.GetTotalQuantity();
            TempData["CartMessage"] = $"{book.Title} sepete eklendi.";

            return Json(new { success = true, message = "Kitap sepete eklendi.", cartCount = cart.GetTotalQuantity() });
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            // Favori işlemi için session kullanılabilir veya kullanıcı girişi yapıldıktan sonra API'ye kayıt edilebilir
            var favoritesJson = HttpContext.Session.GetString("Favorites");
            var favorites = string.IsNullOrEmpty(favoritesJson) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(favoritesJson);

            if (!favorites.Contains(bookId))
            {
                favorites.Add(bookId);
                HttpContext.Session.SetString("Favorites", JsonSerializer.Serialize(favorites));

                var book = await _apiService.GetBookAsync(bookId);
                TempData["FavoriteMessage"] = $"{book?.Title} favorilere eklendi.";

                return Json(new { success = true, message = "Kitap favorilere eklendi." });
            }

            return Json(new { success = false, message = "Kitap zaten favorilerde." });
        }

        public IActionResult Cart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

            ViewData["Title"] = "Sepetim";
            ViewBag.CartTotal = cart.GetTotalPrice();

            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int bookId, int quantity)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

            cart.UpdateQuantity(bookId, quantity);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            return Json(new { success = true, cartTotal = cart.GetTotalPrice(), cartCount = cart.GetTotalQuantity() });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

            cart.RemoveItem(bookId);
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            return Json(new { success = true, cartTotal = cart.GetTotalPrice(), cartCount = cart.GetTotalQuantity() });
        }

        public IActionResult Checkout()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Sepetiniz boş.";
                return RedirectToAction("Cart");
            }

            ViewData["Title"] = "Sipariş Tamamla";
            ViewBag.CartTotal = cart.GetTotalPrice();

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(string customerName, string customerEmail, string shippingAddress)
        {
            try
            {
                var cartJson = HttpContext.Session.GetString("Cart");
                var cart = string.IsNullOrEmpty(cartJson) ? new Cart() : JsonSerializer.Deserialize<Cart>(cartJson);

                if (cart.Items.Count == 0)
                {
                    return Json(new { success = false, message = "Sepetiniz boş." });
                }

                // Create order via API
                var orderItems = cart.Items.Select(item => new
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity
                }).ToList();

                var orderData = new
                {
                    UserId = 2, // Test user ID (gerçek uygulamada session'dan alınır)
                    ShippingAddress = shippingAddress,
                    OrderItems = orderItems
                };

                var json = JsonSerializer.Serialize(orderData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await new HttpClient().PostAsync("https://localhost:7001/api/orders", content);

                if (response.IsSuccessStatusCode)
                {
                    // Clear cart
                    cart.Clear();
                    HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                    TempData["SuccessMessage"] = $"Sayın {customerName}, siparişiniz başarıyla oluşturuldu!";
                    return Json(new { success = true, message = "Sipariş başarıyla tamamlandı." });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Sipariş oluşturulamadı: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Bir hata oluştu: {ex.Message}" });
            }
        }
    }
}