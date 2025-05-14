using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ==================== User Management Endpoints ====================

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một người dùng theo id
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Tạo mới một người dùng
        /// </summary>
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var createdUser = await _adminService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            var updatedUser = await _adminService.UpdateUserAsync(id, user);
            if (updatedUser == null)
                return NotFound();

            return Ok(updatedUser);
        }

        /// <summary>
        /// Xóa một người dùng
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Cập nhật vai trò của người dùng
        /// </summary>
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto model)
        {
            var result = await _adminService.UpdateUserRoleAsync(id, model.Role);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // ==================== Restaurant Management Endpoints ====================

        /// <summary>
        /// Lấy danh sách tất cả nhà hàng
        /// </summary>
        [HttpGet("restaurants")]
        public async Task<IActionResult> GetAllRestaurants()
        {
            var restaurants = await _adminService.GetAllRestaurantsAsync();
            return Ok(restaurants);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một nhà hàng theo id
        /// </summary>
        [HttpGet("restaurants/{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            var restaurant = await _adminService.GetRestaurantByIdAsync(id);
            if (restaurant == null)
                return NotFound();

            return Ok(restaurant);
        }

        /// <summary>
        /// Tạo mới một nhà hàng
        /// </summary>
        [HttpPost("restaurants")]
        public async Task<IActionResult> CreateRestaurant([FromBody] Restaurant restaurant)
        {
            var createdRestaurant = await _adminService.CreateRestaurantAsync(restaurant);
            return CreatedAtAction(nameof(GetRestaurantById), new { id = createdRestaurant.Id }, createdRestaurant);
        }

        /// <summary>
        /// Cập nhật thông tin nhà hàng
        /// </summary>
        [HttpPut("restaurants/{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, [FromBody] Restaurant restaurant)
        {
            var updatedRestaurant = await _adminService.UpdateRestaurantAsync(id, restaurant);
            if (updatedRestaurant == null)
                return NotFound();

            return Ok(updatedRestaurant);
        }

        /// <summary>
        /// Xóa một nhà hàng
        /// </summary>
        [HttpDelete("restaurants/{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var result = await _adminService.DeleteRestaurantAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // ==================== Dashboard Endpoint ====================

        /// <summary>
        /// Lấy thống kê tổng quan cho dashboard admin
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        // ==================== Order Management Endpoints ====================

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _adminService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto model)
        {
            var result = await _adminService.UpdateOrderStatusAsync(id, model.Status);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
