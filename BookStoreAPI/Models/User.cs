﻿using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // User, Admin

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}