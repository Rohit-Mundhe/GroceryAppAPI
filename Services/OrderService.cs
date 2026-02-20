using GroceryOrderingApp.Backend.Models;
using GroceryOrderingApp.Backend.Repositories;

namespace GroceryOrderingApp.Backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<List<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _orderRepository.GetOrdersByUserAsync(userId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order, List<(int productId, int quantity)> items)
        {
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var (productId, quantity) in items)
            {
                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null || !product.IsActive)
                    throw new InvalidOperationException($"Product {productId} not found or inactive");

                if (product.StockQuantity < quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {productId}");

                totalAmount += product.Price * quantity;
                orderItems.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    PriceAtTime = product.Price
                });
            }

            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;
            order.Status = "Pending";

            return await _orderRepository.CreateOrderAsync(order);
        }

        public async Task<bool> DeliverOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.Status != "Pending")
                return false;

            // Reduce stock
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0)
                        product.StockQuantity = 0;
                    await _productRepository.UpdateProductAsync(product);
                }
            }

            order.Status = "Delivered";
            await _orderRepository.UpdateOrderAsync(order);
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.Status != "Pending")
                return false;

            order.Status = "Cancelled";
            await _orderRepository.UpdateOrderAsync(order);
            return true;
        }
    }
}
