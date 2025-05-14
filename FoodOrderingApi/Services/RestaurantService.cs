using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Dịch vụ xử lý nhà hàng và menu
    /// 
    /// Tính năng:
    /// - Quản lý thông tin nhà hàng
    /// - Quản lý menu của nhà hàng
    /// - Tìm kiếm nhà hàng và món ăn
    /// - Phân trang danh sách
    /// </summary>
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;

        public RestaurantService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách nhà hàng có phân trang
        /// </summary>
        public async Task<PagedResult<Restaurant>> GetRestaurantsAsync(int pageNumber = 1, int pageSize = 9)
        {
            var totalItems = await _context.Restaurants.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var restaurants = await _context.Restaurants
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Restaurant>
            {
                Items = restaurants,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhà hàng theo ID
        /// </summary>
        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _context.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Lấy danh sách món ăn của nhà hàng có phân trang
        /// </summary>
        public async Task<PagedResult<MenuItem>> GetMenuItemsAsync(int restaurantId, int pageNumber = 1, int pageSize = 12)
        {
            var totalItems = await _context.MenuItems
                .Where(m => m.RestaurantId == restaurantId)
                .CountAsync();
            
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var menuItems = await _context.MenuItems
                .Where(m => m.RestaurantId == restaurantId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MenuItem>
            {
                Items = menuItems,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Lấy thông tin chi tiết món ăn theo ID
        /// </summary>
        public async Task<MenuItem> GetMenuItemByIdAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Thêm món ăn mới vào nhà hàng
        /// </summary>
        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
        {
            // Kiểm tra nhà hàng tồn tại
            var restaurant = await _context.Restaurants.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
                throw new ArgumentException("Restaurant not found");

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            return menuItem;
        }

        /// <summary>
        /// Cập nhật thông tin món ăn
        /// </summary>
        public async Task<MenuItem> UpdateMenuItemAsync(int id, MenuItem menuItem)
        {
            var existingMenuItem = await _context.MenuItems.FindAsync(id);
            if (existingMenuItem == null)
                return null;

            existingMenuItem.Name = menuItem.Name;
            existingMenuItem.Description = menuItem.Description;
            existingMenuItem.Price = menuItem.Price;

            await _context.SaveChangesAsync();
            return existingMenuItem;
        }

        /// <summary>
        /// Xóa món ăn khỏi nhà hàng
        /// </summary>
        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return false;

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tìm kiếm món ăn trong nhà hàng theo từ khóa
        /// </summary>
        public async Task<IEnumerable<MenuItem>> SearchMenuItemsAsync(int restaurantId, string searchTerm)
        {
            return await _context.MenuItems
                .Where(m => m.RestaurantId == restaurantId && 
                           (m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm)))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm nhà hàng theo từ khóa
        /// </summary>
        public async Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(string searchTerm)
        {
            return await _context.Restaurants
                .Where(r => r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm))
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
    }
} 