namespace BookStoreAPI.DTOs
{
    public class BookCreateDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public int CategoryId { get; set; }
    }
}