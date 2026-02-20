using GroceryOrderingApp.Backend.Data;
using GroceryOrderingApp.Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace GroceryOrderingApp.Backend
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task SeedAsync()
        {
            // Seed Roles
            if (!_context.Roles.Any())
            {
                var roles = new[]
                {
                    new Role { Id = 1, Name = "Admin" },
                    new Role { Id = 2, Name = "Customer" }
                };
                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!_context.Users.Any())
            {
                var adminUser = new User
                {
                    UserId = "admin",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin@123");
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }

            // Seed Categories
            if (!_context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category { Name = "Vegetables", IsActive = true },
                    new Category { Name = "Fruits", IsActive = true },
                    new Category { Name = "Dairy", IsActive = true },
                    new Category { Name = "Grains", IsActive = true },
                    new Category { Name = "Spices", IsActive = true }
                };
                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }

            // Seed Products
            if (!_context.Products.Any())
            {
                var products = new[]
                {
                    // Vegetables
                    new Product { Name = "Tomato", Description = "Fresh red tomato", Price = 40m, StockQuantity = 100, CategoryId = 1, IsActive = true },
                    new Product { Name = "Onion", Description = "Golden onion", Price = 30m, StockQuantity = 150, CategoryId = 1, IsActive = true },
                    new Product { Name = "Potato", Description = "Fresh potato", Price = 25m, StockQuantity = 200, CategoryId = 1, IsActive = true },
                    new Product { Name = "Carrot", Description = "Orange carrot", Price = 35m, StockQuantity = 80, CategoryId = 1, IsActive = true },

                    // Fruits
                    new Product { Name = "Apple", Description = "Red apple", Price = 100m, StockQuantity = 50, CategoryId = 2, IsActive = true },
                    new Product { Name = "Banana", Description = "Yellow banana", Price = 25m, StockQuantity = 120, CategoryId = 2, IsActive = true },
                    new Product { Name = "Orange", Description = "Sweet orange", Price = 50m, StockQuantity = 60, CategoryId = 2, IsActive = true },
                    new Product { Name = "Mango", Description = "Sweet mango", Price = 80m, StockQuantity = 40, CategoryId = 2, IsActive = true },

                    // Dairy
                    new Product { Name = "Milk (1L)", Description = "Fresh milk", Price = 50m, StockQuantity = 200, CategoryId = 3, IsActive = true },
                    new Product { Name = "Yogurt", Description = "Plain yogurt", Price = 35m, StockQuantity = 100, CategoryId = 3, IsActive = true },
                    new Product { Name = "Cheese", Description = "Cheddar cheese", Price = 200m, StockQuantity = 30, CategoryId = 3, IsActive = true },
                    new Product { Name = "Butter", Description = "Fresh butter", Price = 150m, StockQuantity = 50, CategoryId = 3, IsActive = true },

                    // Grains
                    new Product { Name = "Rice (1kg)", Description = "Basmati rice", Price = 80m, StockQuantity = 150, CategoryId = 4, IsActive = true },
                    new Product { Name = "Wheat (1kg)", Description = "Whole wheat flour", Price = 40m, StockQuantity = 200, CategoryId = 4, IsActive = true },
                    new Product { Name = "Oats", Description = "Rolled oats", Price = 120m, StockQuantity = 60, CategoryId = 4, IsActive = true },

                    // Spices
                    new Product { Name = "Turmeric", Description = "Turmeric powder", Price = 45m, StockQuantity = 100, CategoryId = 5, IsActive = true },
                    new Product { Name = "Chili Powder", Description = "Red chili powder", Price = 50m, StockQuantity = 80, CategoryId = 5, IsActive = true },
                    new Product { Name = "Cumin", Description = "Cumin seeds", Price = 60m, StockQuantity = 70, CategoryId = 5, IsActive = true }
                };
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();
            }
        }
    }
}
