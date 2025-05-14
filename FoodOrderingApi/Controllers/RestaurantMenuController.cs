using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers
{
    [Route("api/restaurant")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RestaurantMenuController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantMenuController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Lấy tất cả món ăn của một nhà hàng
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        [HttpGet("{restaurantId}/menuitem")]
        public async Task<IActionResult> GetRestaurantMenuItems(int restaurantId)
        {
            var menuItems = await _restaurantService.GetMenuItemsAsync(restaurantId);
            return Ok(menuItems);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một món ăn của nhà hàng
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="id">Id món ăn</param>
        [HttpGet("{restaurantId}/menuitem/{id}")]
        public async Task<IActionResult> GetMenuItem(int restaurantId, int id)
        {
            var menuItem = await _restaurantService.GetMenuItemByIdAsync(id);
            if (menuItem == null || menuItem.RestaurantId != restaurantId)
                return NotFound();

            return Ok(menuItem);
        }

        /// <summary>
        /// Thêm món ăn mới vào nhà hàng
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="menuItem">Thông tin món ăn</param>
        [HttpPost("{restaurantId}/menuitem")]
        public async Task<IActionResult> AddMenuItem(int restaurantId, [FromBody] MenuItem menuItem)
        {
            // Verify restaurant exists
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
            if (restaurant == null)
                return NotFound("Restaurant not found");

            menuItem.RestaurantId = restaurantId;
            try
            {
                var createdMenuItem = await _restaurantService.CreateMenuItemAsync(menuItem);
                return CreatedAtAction(nameof(GetMenuItem), new { restaurantId, id = createdMenuItem.Id }, createdMenuItem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin một món ăn của nhà hàng
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="id">Id món ăn</param>
        /// <param name="menuItem">Thông tin món ăn cập nhật</param>
        [HttpPut("{restaurantId}/menuitem/{id}")]
        public async Task<IActionResult> UpdateMenuItem(int restaurantId, int id, [FromBody] MenuItem menuItem)
        {
            // Verify menu item exists and belongs to the restaurant
            var existingMenuItem = await _restaurantService.GetMenuItemByIdAsync(id);
            if (existingMenuItem == null || existingMenuItem.RestaurantId != restaurantId)
                return NotFound();

            menuItem.RestaurantId = restaurantId;
            var updatedMenuItem = await _restaurantService.UpdateMenuItemAsync(id, menuItem);
            return Ok(updatedMenuItem);
        }

        /// <summary>
        /// Xóa một món ăn khỏi nhà hàng
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="id">Id món ăn</param>
        [HttpDelete("{restaurantId}/menuitem/{id}")]
        public async Task<IActionResult> DeleteMenuItem(int restaurantId, int id)
        {
            // Verify menu item exists and belongs to the restaurant
            var existingMenuItem = await _restaurantService.GetMenuItemByIdAsync(id);
            if (existingMenuItem == null || existingMenuItem.RestaurantId != restaurantId)
                return NotFound();

            var result = await _restaurantService.DeleteMenuItemAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Tìm kiếm món ăn trong nhà hàng theo từ khóa
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        [HttpGet("{restaurantId}/menuitem/search")]
        public async Task<IActionResult> SearchMenuItems(int restaurantId, [FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var menuItems = await _restaurantService.SearchMenuItemsAsync(restaurantId, searchTerm);
            return Ok(menuItems);
        }
    }
} 