using BookStoreMVC.Models;
using System.Text;
using System.Text.Json;

namespace BookStoreMVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/api";
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Books
        public async Task<List<BookDto>> GetBooksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Books");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<BookDto>>(json, _jsonOptions) ?? new List<BookDto>();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting books: {ex.Message}");
            }
            return new List<BookDto>();
        }

        public async Task<BookDto?> GetBookAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Books/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<BookDto>(json, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book: {ex.Message}");
            }
            return null;
        }

        public async Task<List<BookDto>> GetBooksByCategoryAsync(int categoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Books/category/{categoryId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<BookDto>>(json, _jsonOptions) ?? new List<BookDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting books by category: {ex.Message}");
            }
            return new List<BookDto>();
        }

        public async Task<BookDto?> CreateBookAsync(BookDto book)
        {
            try
            {
                // Temiz bir obje oluştur (navigation property'ler olmadan)
                var cleanBook = new
                {
                    title = book.Title,
                    author = book.Author,
                    description = book.Description,
                    price = book.Price,
                    imageUrl = book.ImageUrl,
                    stock = book.Stock,
                    isActive = book.IsActive,
                    categoryId = book.CategoryId
                };

                var json = JsonSerializer.Serialize(cleanBook, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Yeni DTO endpoint'ini kullan
                var response = await _httpClient.PostAsync($"{_baseUrl}/Books/create", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<BookDto>(responseJson, _jsonOptions);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating book: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> UpdateBookAsync(int id, BookDto book)
        {
            try
            {
                var json = JsonSerializer.Serialize(book, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Books/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Books/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book: {ex.Message}");
            }
            return false;
        }

        // Categories
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Categories");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<CategoryDto>>(json, _jsonOptions) ?? new List<CategoryDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting categories: {ex.Message}");
            }
            return new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Categories/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<CategoryDto>(json, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting category: {ex.Message}");
            }
            return null;
        }

        public async Task<CategoryDto?> CreateCategoryAsync(CategoryDto category)
        {
            try
            {
                Console.WriteLine($"ApiService: Creating category {category.Name}");

                // Create clean object
                var cleanCategory = new
                {
                    name = category.Name,
                    description = category.Description,
                    imageUrl = category.ImageUrl,
                    isActive = category.IsActive
                };

                var json = JsonSerializer.Serialize(cleanCategory, _jsonOptions);
                Console.WriteLine($"JSON to send: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending POST request to: {_baseUrl}/Categories");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Categories", content);

                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response JSON: {responseJson}");

                    var result = JsonSerializer.Deserialize<CategoryDto>(responseJson, _jsonOptions);
                    Console.WriteLine($"Deserialized result: {result?.Name}");
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateCategoryAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> UpdateCategoryAsync(int id, CategoryDto category)
        {
            try
            {
                var json = JsonSerializer.Serialize(category, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Categories/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Categories/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category: {ex.Message}");
            }
            return false;
        }
    }
}