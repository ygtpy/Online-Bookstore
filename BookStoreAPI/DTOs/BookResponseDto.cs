// DTOs/BookResponseDto.cs
namespace BookStoreAPI.DTOs
{
    public class BookResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CategoryId { get; set; }
        public CategoryResponseDto? Category { get; set; }
    }
}