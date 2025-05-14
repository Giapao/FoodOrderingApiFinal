using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrderingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Tạo đơn hàng mới từ dữ liệu truyền lên (không qua giỏ hàng)
        /// </summary>
        /// <param name="orderDto">Thông tin đơn hàng</param>
        /// <returns>Đơn hàng vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            // Lấy userId từ token xác thực
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            orderDto.UserId = userId;
            var order = await _orderService.CreateOrder(orderDto);
            // Trả về thông tin đơn hàng vừa tạo
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Tạo đơn hàng mới từ giỏ hàng (cart)
        /// </summary>
        /// <param name="cartId">Id của giỏ hàng</param>
        /// <param name="dto">Thông tin giao hàng</param>
        /// <returns>Đơn hàng vừa tạo</returns>
        [HttpPost("from-cart/{cartId}")]
        public async Task<IActionResult> CreateOrderFromCart(int cartId, [FromBody] CreateOrderFromCartDto dto)
        {
            var order = await _orderService.CreateOrderFromCart(cartId, dto.SpecialInstructions, dto.PhoneNumber, dto.DeliveryAddress);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một đơn hàng theo id
        /// </summary>
        /// <param name="id">Id đơn hàng</param>
        /// <returns>Thông tin đơn hàng</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderById(id);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của user hiện tại
        /// </summary>
        [HttpGet("user")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var orders = await _orderService.GetOrdersByUser(userId);
            return Ok(orders);
        }

        /// <summary>
        /// (Admin) Lấy danh sách đơn hàng của một nhà hàng
        /// </summary>
        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRestaurantOrders(int restaurantId)
        {
            var orders = await _orderService.GetOrdersByRestaurant(restaurantId);
            return Ok(orders);
        }

        /// <summary>
        /// (Admin) Lấy danh sách đơn hàng theo trạng thái
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatus(status);
            return Ok(orders);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (chỉ chủ đơn hàng được phép)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var order = await _orderService.UpdateOrderStatus(id, status, userId);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Hủy đơn hàng (chỉ chủ đơn hàng được phép)
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string cancellationReason)
        {
            try
            {
                var order = await _orderService.CancelOrder(id, cancellationReason);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    /// <summary>
    /// DTO dùng để nhận thông tin giao hàng khi tạo đơn từ giỏ hàng
    /// </summary>
    public class CreateOrderFromCartDto
    {
        public string? SpecialInstructions { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
