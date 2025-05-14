using FoodOrderingApi.DTOs;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace FoodOrderingApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Lấy userId từ token xác thực (hỗ trợ nhiều loại claim)
        /// </summary>
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? User.FindFirst("nameid")?.Value;

            Log.Information("All claims: {@Claims}", User.Claims.Select(c => new { c.Type, c.Value }));
            Log.Information("UserId claim value: {UserIdClaim}", userIdClaim);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Log.Warning("Invalid userId claim: {UserIdClaim}", userIdClaim);
                throw new UnauthorizedAccessException("Invalid user ID");
            }

            return userId;
        }

        /// <summary>
        /// Lấy giỏ hàng hiện tại của user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Thêm món ăn vào giỏ hàng của user tại nhà hàng chỉ định
        /// </summary>
        [HttpPost("restaurant/{restaurantId}")]
        public async Task<ActionResult<CartDto>> AddToCart(int restaurantId, [FromBody] AddToCartDto dto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.AddToCartAsync(userId, restaurantId, dto);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding to cart");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật số lượng hoặc ghi chú cho một món trong giỏ hàng
        /// </summary>
        [HttpPut("items/{cartItemId}")]
        public async Task<ActionResult<CartDto>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.UpdateCartItemAsync(userId, cartItemId, dto);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating cart item");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Xóa một món khỏi giỏ hàng
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        public async Task<ActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng của user
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.ClearCartAsync(userId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
} 