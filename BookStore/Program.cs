using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BookStoreAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Database Configuration
builder.Services.AddDbContext<BookStoreDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "BookStore API",
        Description = "A simple example ASP.NET Core Web API for BookStore",
        Contact = new OpenApiContact
        {
            Name = "BookStore API",
            Email = "info@bookstore.com"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BookStoreDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (!context.Categories.Any())
        {
            Console.WriteLine("Adding sample categories...");
            // Add sample data
            var categories = new[]
            {
                new BookStoreAPI.Models.Category { Name = "Roman", Description = "Roman kitaplarý", CreatedDate = DateTime.Now },
                new BookStoreAPI.Models.Category { Name = "Bilim Kurgu", Description = "Bilim kurgu kitaplarý", CreatedDate = DateTime.Now },
                new BookStoreAPI.Models.Category { Name = "Tarih", Description = "Tarih kitaplarý", CreatedDate = DateTime.Now },
                new BookStoreAPI.Models.Category { Name = "Felsefe", Description = "Felsefe kitaplarý", CreatedDate = DateTime.Now }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Categories added successfully.");
        }

        if (!context.Books.Any())
        {
            Console.WriteLine("Adding sample books...");
            // Add sample books
            var books = new[]
            {
                new BookStoreAPI.Models.Book
                {
                    Title = "Suç ve Ceza",
                    Author = "Fyodor Dostoyevski",
                    Description = "Klasik Rus edebiyatýnýn önemli eserlerinden biri.",
                    Price = 45.50m,
                    Stock = 10,
                    CategoryId = 1,
                    CreatedDate = DateTime.Now
                },
                new BookStoreAPI.Models.Book
                {
                    Title = "Dune",
                    Author = "Frank Herbert",
                    Description = "Bilim kurgu edebiyatýnýn baþyapýtlarýndan biri.",
                    Price = 65.00m,
                    Stock = 15,
                    CategoryId = 2,
                    CreatedDate = DateTime.Now
                },
                new BookStoreAPI.Models.Book
                {
                    Title = "Sapiens",
                    Author = "Yuval Noah Harari",
                    Description = "Ýnsanlýðýn tarihini anlatan etkileyici bir eser.",
                    Price = 55.75m,
                    Stock = 8,
                    CategoryId = 3,
                    CreatedDate = DateTime.Now
                }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
            Console.WriteLine("Books added successfully.");
        }

        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

Console.WriteLine("BookStore API is starting...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("Navigate to https://localhost:7001 to access Swagger UI");

app.Run();