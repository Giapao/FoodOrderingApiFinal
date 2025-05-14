using FoodOrderingApi.Data;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức xử lý giỏ hàng
    /// </summary>
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task<CartDto> GetCartById(int cartId);
        Task<CartDto> AddToCartAsync(int userId, int restaurantId, AddToCartDto dto);
        Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
    }

    /// <summary>
    /// Lớp dịch vụ xử lý tất cả các hoạt động liên quan đến giỏ hàng
    /// Các tính năng chính:
    /// - Quản lý giỏ hàng của người dùng
    /// - Thêm/sửa/xóa món ăn trong giỏ hàng
    /// - Tính toán tổng giá trị giỏ hàng
    /// - Xử lý ghi chú đặc biệt cho từng món
    /// - Kiểm tra và cập nhật số lượng món ăn
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin giỏ hàng hiện tại của người dùng
        /// 1. Tìm giỏ hàng theo userId
        /// 2. Load thông tin nhà hàng và các món ăn
        /// 3. Tính toán tổng giá trị giỏ hàng
        /// 4. Trả về thông tin giỏ hàng dưới dạng DTO
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Thông tin giỏ hàng dưới dạng CartDto</returns>
        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Restaurant)
                .Include(c => c.Items)
                    .ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return new CartDto();
            }

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                RestaurantId = cart.RestaurantId,
                RestaurantName = cart.Restaurant.Name,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    SpecialInstructions = i.SpecialInstructions
                }).ToList(),
                TotalPrice = cart.TotalPrice
            };
        }

        /// <summary>
        /// Lấy thông tin giỏ hàng theo ID
        /// 1. Tìm giỏ hàng theo cartId
        /// 2. Load thông tin nhà hàng và các món ăn
        /// 3. Nếu không tìm thấy, throw exception
        /// 4. Trả về thông tin giỏ hàng dưới dạng DTO
        /// </summary>
        /// <param name="cartId">ID của giỏ hàng</param>
        /// <returns>Thông tin giỏ hàng dưới dạng CartDto</returns>
        /// <exception cref="Exception">Khi không tìm thấy giỏ hàng</exception>
        public async Task<CartDto> GetCartById(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Restaurant)
                .Include(c => c.Items)
                    .ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                RestaurantId = cart.RestaurantId,
                RestaurantName = cart.Restaurant.Name,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    SpecialInstructions = i.SpecialInstructions
                }).ToList(),
                TotalPrice = cart.TotalPrice
            };
        }

        /// <summary>
        /// Thêm món ăn vào giỏ hàng
        /// 1. Kiểm tra món ăn tồn tại
        /// 2. Tìm giỏ hàng hiện tại của người dùng tại nhà hàng
        /// 3. Nếu chưa có giỏ hàng, tạo mới
        /// 4. Kiểm tra món đã có trong giỏ chưa:
        ///    - Nếu có: cập nhật số lượng và ghi chú
        ///    - Nếu chưa: thêm món mới vào giỏ
        /// 5. Cập nhật tổng giá trị giỏ hàng
        /// 6. Lưu thay đổi và trả về thông tin giỏ hàng mới
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="restaurantId">ID của nhà hàng</param>
        /// <param name="dto">Thông tin món ăn cần thêm</param>
        /// <returns>Thông tin giỏ hàng sau khi thêm món</returns>
        /// <exception cref="Exception">Khi không tìm thấy món ăn</exception>
        public async Task<CartDto> AddToCartAsync(int userId, int restaurantId, AddToCartDto dto)
        {
            var menuItem = await _context.MenuItems.FindAsync(dto.MenuItemId);
            if (menuItem == null)
            {
                throw new Exception("Menu item not found");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.RestaurantId == restaurantId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    RestaurantId = restaurantId,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == dto.MenuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                existingItem.Price = menuItem.Price * existingItem.Quantity;
                existingItem.SpecialInstructions = dto.SpecialInstructions;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    MenuItemId = dto.MenuItemId,
                    Quantity = dto.Quantity,
                    Price = menuItem.Price * dto.Quantity,
                    SpecialInstructions = dto.SpecialInstructions
                });
            }

            cart.TotalPrice = cart.Items.Sum(i => i.Price);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        /// <summary>
        /// Cập nhật thông tin món ăn trong giỏ hàng
        /// 1. Tìm món ăn trong giỏ hàng
        /// 2. Cập nhật số lượng và ghi chú
        /// 3. Tính lại giá tiền dựa trên số lượng mới
        /// 4. Cập nhật tổng giá trị giỏ hàng
        /// 5. Lưu thay đổi và trả về thông tin giỏ hàng mới
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="cartItemId">ID của món ăn trong giỏ</param>
        /// <param name="dto">Thông tin cập nhật</param>
        /// <returns>Thông tin giỏ hàng sau khi cập nhật</returns>
        /// <exception cref="Exception">Khi không tìm thấy món ăn trong giỏ</exception>
        public async Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
        {
            var cartItem = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.MenuItem)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            cartItem.Quantity = dto.Quantity;
            cartItem.Price = cartItem.MenuItem.Price * dto.Quantity;
            cartItem.SpecialInstructions = dto.SpecialInstructions;

            cartItem.Cart.TotalPrice = cartItem.Cart.Items.Sum(i => i.Price);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        /// <summary>
        /// Xóa món ăn khỏi giỏ hàng
        /// 1. Tìm món ăn trong giỏ hàng
        /// 2. Xóa món ăn khỏi giỏ
        /// 3. Cập nhật tổng giá trị giỏ hàng
        /// 4. Lưu thay đổi
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="cartItemId">ID của món ăn trong giỏ</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy món ăn</returns>
        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (cartItem == null)
            {
                return false;
            }

            _context.CartItems.Remove(cartItem);
            cartItem.Cart.TotalPrice = cartItem.Cart.Items.Sum(i => i.Price);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa toàn bộ món ăn trong giỏ hàng
        /// 1. Tìm giỏ hàng của người dùng
        /// 2. Xóa tất cả món ăn trong giỏ
        /// 3. Đặt lại tổng giá trị giỏ hàng về 0
        /// 4. Lưu thay đổi
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy giỏ hàng</returns>
        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return false;
            }

            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.TotalPrice = 0;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
} 