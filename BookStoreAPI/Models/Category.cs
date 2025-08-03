﻿using System.ComponentModel.DataAnnotations;


namespace BookStoreAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}