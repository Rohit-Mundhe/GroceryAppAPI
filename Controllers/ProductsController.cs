using Microsoft.AspNetCore.Mvc;
using GroceryOrderingApp.Backend.Repositories;
using GroceryOrderingApp.Backend.DTOs;

namespace GroceryOrderingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] int? categoryId = null)
        {
            List<Models.Product> products;

            // If categoryId is not provided (or invalid), return all active products.
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = await _productRepository.GetActiveProductsByCategoryAsync(categoryId.Value);
            }
            else
            {
                products = (await _productRepository.GetAllProductsAsync())
                    .Where(p => p.IsActive)
                    .ToList();
            }

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                IsActive = p.IsActive
            }).ToList();

            return Ok(productDtos);
        }
    }
}
