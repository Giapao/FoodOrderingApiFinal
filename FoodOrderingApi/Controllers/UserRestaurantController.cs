using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers
{
    [Route("api/user/restaurants")]
    [ApiController]
    [Authorize]
    public class UserRestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public UserRestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Lấy danh sách nhà hàng (có phân trang)
        /// </summary>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <param name="pageSize">Số lượng nhà hàng mỗi trang (mặc định 9)</param>
        [HttpGet]
        public async Task<IActionResult> GetRestaurants([FromQuery] int page = 1, [FromQuery] int pageSize = 9)
        {
            var restaurants = await _restaurantService.GetRestaurantsAsync(page, pageSize);
            return Ok(restaurants);
        }

        /// <summary>
        /// Lấy chi tiết một nhà hàng (bao gồm menu)
        /// </summary>
        /// <param name="id">Id nhà hàng</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantDetails(int id)
        {
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        /// <summary>
        /// Tìm kiếm nhà hàng theo tên hoặc mô tả
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        [HttpGet("search")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var restaurants = await _restaurantService.SearchRestaurantsAsync(searchTerm);
            return Ok(restaurants);
        }

        /// <summary>
        /// Lấy danh sách món ăn của một nhà hàng (có phân trang)
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <param name="pageSize">Số lượng món ăn mỗi trang (mặc định 12)</param>
        [HttpGet("{restaurantId}/menu")]
        public async Task<IActionResult> GetRestaurantMenu(int restaurantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var menuItems = await _restaurantService.GetMenuItemsAsync(restaurantId, page, pageSize);
            return Ok(menuItems);
        }

        /// <summary>
        /// Tìm kiếm món ăn trong nhà hàng theo từ khóa
        /// </summary>
        /// <param name="restaurantId">Id nhà hàng</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        [HttpGet("{restaurantId}/menu/search")]
        public async Task<IActionResult> SearchMenuItems(int restaurantId, [FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var menuItems = await _restaurantService.SearchMenuItemsAsync(restaurantId, searchTerm);
            return Ok(menuItems);
        }
    }
} 