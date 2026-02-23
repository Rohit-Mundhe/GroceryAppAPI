using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GroceryOrderingApp.Backend.Repositories;
using GroceryOrderingApp.Backend.Models;
using GroceryOrderingApp.Backend.DTOs;
using GroceryOrderingApp.Backend.Services;
using Microsoft.AspNetCore.Identity;

namespace GroceryOrderingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AdminController(
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IOrderService orderService)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _passwordHasher = new PasswordHasher<User>();
        }

        // Users Management
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.MobileNumber) ||
                string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest("UserId, Password, FullName, MobileNumber, and Address are required");
            }

            var existingUser = await _userRepository.GetUserByUserIdAsync(request.UserId);
            if (existingUser != null)
                return BadRequest("UserId already exists");

            var role = request.Role?.ToLower() == "admin" ? "Admin" : "Customer";
            var roles = await _userRepository.GetAllUsersAsync();
            
            var roleId = 2; // Default to Customer
            if (role == "Admin")
                roleId = 1;

            var user = new User
            {
                UserId = request.UserId,
                FullName = request.FullName,
                MobileNumber = request.MobileNumber,
                Address = request.Address,
                RoleId = roleId,
                CreatedBy = int.Parse(User.FindFirst("userId")?.Value ?? "0"),
                IsActive = true
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            var createdUser = await _userRepository.CreateUserAsync(user);

            var userDto = new UserDto
            {
                Id = createdUser.Id,
                UserId = createdUser.UserId,
                Role = role,
                FullName = createdUser.FullName,
                MobileNumber = createdUser.MobileNumber,
                Address = createdUser.Address,
                CreatedAt = createdUser.CreatedAt,
                IsActive = createdUser.IsActive,
                CreatedBy = createdUser.CreatedBy
            };

            return Created($"/api/admin/users/{createdUser.Id}", userDto);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserId = u.UserId,
                Role = u.Role?.Name ?? "Customer",
                FullName = u.FullName,
                MobileNumber = u.MobileNumber,
                Address = u.Address,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                CreatedBy = u.CreatedBy
            }).ToList();

            return Ok(userDtos);
        }

        // Categories Management
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Category name is required");

            var category = new Category { Name = request.Name, IsActive = true };
            var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

            var categoryDto = new CategoryDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                IsActive = createdCategory.IsActive
            };

            return Created($"/api/admin/categories/{createdCategory.Id}", categoryDto);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound("Category not found");

            category.Name = request.Name;
            category.IsActive = request.IsActive;

            await _categoryRepository.UpdateCategoryAsync(category);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            };

            return Ok(categoryDto);
        }

        // Products Management
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || request.StockQuantity < 0)
                return BadRequest("Invalid product data");

            var category = await _categoryRepository.GetCategoryByIdAsync(request.CategoryId);
            if (category == null)
                return BadRequest("Category not found");

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                IsActive = true
            };

            var createdProduct = await _productRepository.CreateProductAsync(product);

            var productDto = new ProductDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                StockQuantity = createdProduct.StockQuantity,
                CategoryId = createdProduct.CategoryId,
                IsActive = createdProduct.IsActive
            };

            return Created($"/api/admin/products/{createdProduct.Id}", productDto);
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
                return NotFound("Product not found");

            if (request.Price < 0 || request.StockQuantity < 0)
                return BadRequest("Invalid product data");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.CategoryId = request.CategoryId;
            product.IsActive = request.IsActive;

            await _productRepository.UpdateProductAsync(product);

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };

            return Ok(productDto);
        }

        // Orders Management
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserFullName = o.User?.FullName ?? o.User?.UserId ?? string.Empty,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    PriceAtTime = oi.PriceAtTime
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound("Order not found");

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserFullName = order.User?.FullName ?? order.User?.UserId ?? string.Empty,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    PriceAtTime = oi.PriceAtTime
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPut("orders/{id}/deliver")]
        public async Task<IActionResult> DeliverOrder(int id)
        {
            var success = await _orderService.DeliverOrderAsync(id);
            if (!success)
                return BadRequest("Cannot deliver this order");

            return Ok(new { message = "Order delivered successfully" });
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var success = await _orderService.CancelOrderAsync(id);
            if (!success)
                return BadRequest("Cannot cancel this order");

            return Ok(new { message = "Order cancelled successfully" });
        }
    }
}
