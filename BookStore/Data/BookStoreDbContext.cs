using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Models;

namespace BookStoreAPI.Data
{
    public class BookStoreDbContext : DbContext
    {
        public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasDefaultValue("User");
            });

            // Book Configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Books)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Book)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Favorite Configuration
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Book)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.BookId }).IsUnique();
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Roman", Description = "Roman kitapları", CreatedDate = new DateTime(2023, 1, 1) },
                new Category { Id = 2, Name = "Bilim Kurgu", Description = "Bilim kurgu kitapları", CreatedDate = new DateTime(2023, 1, 1) },
                new Category { Id = 3, Name = "Tarih", Description = "Tarih kitapları", CreatedDate = new DateTime(2023, 1, 1) },
                new Category { Id = 4, Name = "Felsefe", Description = "Felsefe kitapları", CreatedDate = new DateTime(2023, 1, 1) }
            );

            // Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@bookstore.com",
                    Password = "admin123", // In real app, this should be hashed
                    Role = "Admin",
                    CreatedDate = new DateTime(2023, 1, 1)
                },
                new User
                {
                    Id = 2,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "user@bookstore.com",
                    Password = "user123", // In real app, this should be hashed
                    Role = "User",
                    CreatedDate = new DateTime(2023, 1, 1)
                }
            );
        }
    }
}