using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GroceryOrderingApp.Backend.Models;
using GroceryOrderingApp.Backend.Repositories;
using GroceryOrderingApp.Backend.Services;
using GroceryOrderingApp.Backend.DTOs;
using System.Security.Claims;

namespace GroceryOrderingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrdersController(
            IOrderService orderService,
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
                return BadRequest("Order must contain at least one item");

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    DeliveryAddress = request.DeliveryAddress?.Trim()
                };
                var items = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();

                var createdOrder = await _orderService.CreateOrderAsync(order, items);

                var orderDto = new OrderDto
                {
                    Id = createdOrder.Id,
                    UserId = createdOrder.UserId,
                    OrderDate = createdOrder.OrderDate,
                    Status = createdOrder.Status,
                    TotalAmount = createdOrder.TotalAmount,
                    DeliveryAddress = createdOrder.DeliveryAddress,
                    Items = createdOrder.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "",
                        Quantity = oi.Quantity,
                        PriceAtTime = oi.PriceAtTime
                    }).ToList()
                };

                return Created($"/api/orders/{createdOrder.Id}", orderDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var orders = await _orderService.GetOrdersByUserAsync(userId);
            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.UserId != userId)
                return NotFound("Order not found");

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserFullName = order.User?.FullName ?? string.Empty,
                UserMobileNumber = order.User?.MobileNumber ?? string.Empty,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DeliveryAddress = order.DeliveryAddress,
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
    }
}
