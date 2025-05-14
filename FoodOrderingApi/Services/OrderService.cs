using FoodOrderingApi.Data;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Dịch vụ xử lý đơn hàng
    /// 
    /// Tính năng:
    /// - Tạo đơn hàng mới
    /// - Tạo đơn từ giỏ hàng
    /// - Quản lý trạng thái đơn hàng
    /// - Gửi email thông báo
    /// - Hủy đơn hàng
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IEmailService _emailService;

        public OrderService(ApplicationDbContext context, ICartService cartService, IEmailService emailService)
        {
            _context = context;
            _cartService = cartService;
            _emailService = emailService;
        }

        /// <summary>
        /// Tạo đơn hàng mới từ thông tin được cung cấp
        /// </summary>
        public async Task<OrderDto> CreateOrder(OrderDto orderDto)
        {
            var order = new Order
            {
                UserId = orderDto.UserId,
                RestaurantId = orderDto.RestaurantId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = orderDto.OrderDetails.Sum(d => d.Quantity * d.UnitPrice),
                PhoneNumber = orderDto.PhoneNumber,
                DeliveryAddress = orderDto.DeliveryAddress,
                SpecialInstructions = orderDto.SpecialInstructions,
                OrderDetails = orderDto.OrderDetails.Select(d => new OrderDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        /// <summary>
        /// Tạo đơn hàng mới từ giỏ hàng
        /// </summary>
        public async Task<OrderDto> CreateOrderFromCart(int cartId, string? specialInstructions, string phoneNumber, string deliveryAddress)
        {
            var cart = await _cartService.GetCartById(cartId);
            if (cart == null)
                throw new ArgumentException("Cart not found");

            var order = new Order
            {
                UserId = cart.UserId,
                RestaurantId = cart.RestaurantId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = cart.TotalPrice,
                PhoneNumber = phoneNumber,
                DeliveryAddress = deliveryAddress,
                SpecialInstructions = specialInstructions,
                OrderDetails = cart.Items.Select(item => new OrderDetail
                {
                    ProductId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi tạo đơn
            await _cartService.ClearCartAsync(cart.UserId);

            return MapToDto(order);
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của người dùng
        /// </summary>
        public async Task<List<OrderDto>> GetOrdersByUser(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng theo ID
        /// </summary>
        public async Task<OrderDto> GetOrderById(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException("Order not found");

            return MapToDto(order);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// - Chỉ cho phép chuyển từ Pending sang Confirmed
        /// - Gửi email thông báo khi xác nhận đơn
        /// </summary>
        public async Task<OrderDto> UpdateOrderStatus(int orderId, string status, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        
            if (order == null)
                throw new ArgumentException("Order not found");

            // Kiểm tra quyền cập nhật
            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own orders");

            // Kiểm tra trạng thái hợp lệ
            if (order.Status != "Pending" && status == "Confirmed")
                throw new InvalidOperationException("Only pending orders can be confirmed");

            order.Status = status;
            
            // Cập nhật thời gian theo trạng thái
            switch (status.ToLower())
            {
                case "confirmed":
                    order.ConfirmedAt = DateTime.UtcNow;
                    // Gửi email xác nhận
                    if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
                    {
                        var subject = "Order Confirmation";
                        var body = $@"
                            <h2>Your Order Has Been Confirmed</h2>
                            <p>Dear Customer,</p>
                            <p>Your order #{order.Id} has been confirmed.</p>
                            <p>Order Details:</p>
                            <ul>
                                <li>Order ID: #{order.Id}</li>
                                <li>Total Amount: ${order.TotalAmount}</li>
                                <li>Delivery Address: {order.DeliveryAddress}</li>
                                <li>Status: {order.Status}</li>
                            </ul>
                            <p>Thank you for choosing our service!</p>";
                        
                        try
                        {
                            await _emailService.SendEmailAsync(order.User.Email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
                        }
                    }
                    break;
                case "preparing":
                    order.PreparedAt = DateTime.UtcNow;
                    break;
                case "completed":
                    order.CompletedAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();
            return MapToDto(order);
        }

        /// <summary>
        /// Hủy đơn hàng
        /// - Không cho phép hủy đơn đã hoàn thành hoặc đã hủy
        /// - Lưu lý do hủy đơn
        /// </summary>
        public async Task<OrderDto> CancelOrder(int orderId, string cancellationReason)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            if (order.Status == "Completed" || order.Status == "Cancelled")
                throw new InvalidOperationException("Cannot cancel a completed or already cancelled order");

            order.Status = "Cancelled";
            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = cancellationReason;

            await _context.SaveChangesAsync();
            return MapToDto(order);
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của nhà hàng
        /// </summary>
        public async Task<List<OrderDto>> GetOrdersByRestaurant(int restaurantId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Lấy danh sách đơn hàng theo trạng thái
        /// </summary>
        public async Task<List<OrderDto>> GetOrdersByStatus(string status)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Chuyển đổi Order thành OrderDto
        /// </summary>
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                RestaurantId = order.RestaurantId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                PhoneNumber = order.PhoneNumber,
                DeliveryAddress = order.DeliveryAddress,
                SpecialInstructions = order.SpecialInstructions,
                ConfirmedAt = order.ConfirmedAt,
                PreparedAt = order.PreparedAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt,
                CancellationReason = order.CancellationReason,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    ProductName = od.Product != null ? od.Product.Name : string.Empty
                }).ToList()
            };
        }
    }
}
