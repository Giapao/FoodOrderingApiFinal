using FoodOrderingApi.Data;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services
{
    public interface IAdminService
    {
        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();
        /// <summary>
        /// Lấy thông tin chi tiết một người dùng theo id
        /// </summary>
        Task<User> GetUserByIdAsync(int id);
        /// <summary>
        /// Tạo mới một người dùng
        /// </summary>
        Task<User> CreateUserAsync(User user);
        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        Task<User> UpdateUserAsync(int id, User user);
        /// <summary>
        /// Xóa một người dùng
        /// </summary>
        Task<bool> DeleteUserAsync(int id);
        /// <summary>
        /// Lấy danh sách tất cả nhà hàng
        /// </summary>
        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();
        /// <summary>
        /// Lấy thông tin chi tiết một nhà hàng theo id
        /// </summary>
        Task<Restaurant> GetRestaurantByIdAsync(int id);
        /// <summary>
        /// Tạo mới một nhà hàng
        /// </summary>
        Task<Restaurant> CreateRestaurantAsync(Restaurant restaurant);
        /// <summary>
        /// Cập nhật thông tin nhà hàng
        /// </summary>
        Task<Restaurant> UpdateRestaurantAsync(int id, Restaurant restaurant);
        /// <summary>
        /// Xóa một nhà hàng
        /// </summary>
        Task<bool> DeleteRestaurantAsync(int id);
        /// <summary>
        /// Lấy thống kê tổng quan cho dashboard admin
        /// </summary>
        Task<DashboardStats> GetDashboardStatsAsync();
        /// <summary>
        /// Cập nhật vai trò của người dùng
        /// </summary>
        Task<bool> UpdateUserRoleAsync(int id, string newRole);
        /// <summary>
        /// Lấy danh sách tất cả đơn hàng
        /// </summary>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(int id, string newStatus);
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <inheritdoc/>
        public async Task<User> UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.EmailConfirmed = user.EmailConfirmed;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
                
            if (user == null)
                return false;

            // Xóa tất cả các cart liên quan
            var carts = await _context.Carts
                .Where(c => c.UserId == id)
                .ToListAsync();
            _context.Carts.RemoveRange(carts);

            // Xóa tất cả các order liên quan
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .ToListAsync();
            _context.Orders.RemoveRange(orders);

            // Cuối cùng xóa user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync()
        {
            return await _context.Restaurants.Include(r => r.MenuItems).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _context.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Restaurant> CreateRestaurantAsync(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        /// <inheritdoc/>
        public async Task<Restaurant> UpdateRestaurantAsync(int id, Restaurant restaurant)
        {
            var existingRestaurant = await _context.Restaurants.FindAsync(id);
            if (existingRestaurant == null)
                return null;

            existingRestaurant.Name = restaurant.Name;
            existingRestaurant.Description = restaurant.Description;
            existingRestaurant.Address = restaurant.Address;
            existingRestaurant.PhoneNumber = restaurant.PhoneNumber;

            await _context.SaveChangesAsync();
            return existingRestaurant;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteRestaurantAsync(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return false;

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalRestaurants = await _context.Restaurants.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            
            // Tính tổng doanh thu từ các đơn đã hoàn thành
            var completedOrders = await _context.Orders
                .Where(o => o.Status.ToLower() == "completed")
                .ToListAsync();
            
            var totalRevenue = completedOrders.Sum(o => o.TotalAmount);

            return new DashboardStats
            {
                TotalUsers = totalUsers,
                TotalRestaurants = totalRestaurants,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserRoleAsync(int id, string newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Role = newRole;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserEmail = o.User.Email,
                    RestaurantId = o.RestaurantId,
                    RestaurantName = o.Restaurant.Name,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    DeliveryAddress = o.DeliveryAddress,
                    PhoneNumber = o.PhoneNumber,
                    SpecialInstructions = o.SpecialInstructions
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateOrderStatusAsync(int id, string newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return false;

            order.Status = newStatus;

            // Cập nhật thời gian dựa trên trạng thái
            switch (newStatus.ToLower())
            {
                case "confirmed":
                    order.ConfirmedAt = DateTime.UtcNow;
                    break;
                case "preparing":
                    order.PreparedAt = DateTime.UtcNow;
                    break;
                case "completed":
                    order.CompletedAt = DateTime.UtcNow;
                    break;
                case "cancelled":
                    order.CancelledAt = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();

            // Gửi email thông báo
            if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
            {
                var subject = $"Order #{order.Id} Status Update";
                var body = $@"
                    <h2>Your Order Status Has Been Updated</h2>
                    <p>Dear Customer,</p>
                    <p>Your order #{order.Id} from {order.Restaurant.Name} has been updated to: <strong>{newStatus}</strong></p>
                    <p>Order Details:</p>
                    <ul>
                        <li>Order ID: #{order.Id}</li>
                        <li>Restaurant: {order.Restaurant.Name}</li>
                        <li>Total Amount: ${order.TotalAmount}</li>
                        <li>Delivery Address: {order.DeliveryAddress}</li>
                        <li>New Status: {newStatus}</li>
                    </ul>
                    <p>Thank you for choosing our service!</p>";

                try
                {
                    await _emailService.SendEmailAsync(order.User.Email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending status update email: {ex.Message}");
                }
            }

            return true;
        }
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalRestaurants { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
} 