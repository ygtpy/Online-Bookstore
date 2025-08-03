﻿// Models/CategoryDto.cs
namespace BookStoreMVC.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public List<BookDto> Books { get; set; } = new List<BookDto>();
    }
}