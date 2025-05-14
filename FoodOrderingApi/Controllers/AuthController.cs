using FoodOrderingApi.DTOs;
using FoodOrderingApi.DTOs.Auth;
using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;

namespace FoodOrderingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _authService = authService;
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                var result = await _authService.RegisterAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        /// <summary>
        /// Đăng ký tài khoản admin (chỉ admin mới được phép)
        /// </summary>
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto model)
        {
            try
            {
                var result = await _authService.RegisterAdminAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var result = await _authService.LoginAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        /// <summary>
        /// Đăng xuất tài khoản hiện tại
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    await _authService.LogoutAsync(int.Parse(userId));
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout", error = ex.Message });
            }
        }

        /// <summary>
        /// Làm mới token truy cập
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while refreshing token", error = ex.Message });
            }
        }

        /// <summary>
        /// Gửi email quên mật khẩu
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing forgot password request", error = ex.Message });
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu bằng token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while resetting password", error = ex.Message });
            }
        }

        /// <summary>
        /// Xác thực email người dùng (GET)
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmailGet([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                var model = new VerifyEmailDto { Token = token, Email = email };
                var result = await _authService.VerifyEmailAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                // Redirect to a success page or return a success message
                return Ok(new { message = "Email verified successfully! You can now login." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying email", error = ex.Message });
            }
        }

        /// <summary>
        /// Xác thực email người dùng (POST)
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmailPost([FromBody] VerifyEmailDto model)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying email", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái xác thực email
        /// </summary>
        [HttpGet("check-email-verification")]
        public async Task<IActionResult> CheckEmailVerification([FromQuery] string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { 
                    email = user.Email,
                    isEmailConfirmed = user.EmailConfirmed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking email verification status", error = ex.Message });
            }
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản hiện tại
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _authService.ChangePasswordAsync(userId, model);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing password", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra token hợp lệ và lấy thông tin user
        /// </summary>
        [HttpGet("validate-token")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _userService.GetUserByIdAsync(int.Parse(userId));
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                return Ok(new { user = _mapper.Map<UserDto>(user) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while validating token", error = ex.Message });
            }
        }
    }
}
