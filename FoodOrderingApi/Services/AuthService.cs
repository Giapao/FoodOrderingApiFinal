using AutoMapper;
using FoodOrderingApi.Data;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.DTOs.Auth;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Lớp dịch vụ xử lý tất cả các hoạt động liên quan đến xác thực và phân quyền
    /// Bao gồm: đăng ký người dùng, đăng nhập, quản lý mật khẩu, và xác thực email.
    /// 
    /// Các tính năng chính:
    /// - Đăng ký và xác thực người dùng
    /// - Quản lý phiên đăng nhập với JWT và Refresh Token
    /// - Xử lý quên mật khẩu và đặt lại mật khẩu
    /// - Xác thực email
    /// - Phân quyền người dùng (User/Admin)
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            ApplicationDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        /// <summary>
        /// Đăng ký người dùng mới với thông tin được cung cấp.
        /// Gửi email xác thực đến địa chỉ email của người dùng.
        /// </summary>
        /// <param name="model">Thông tin đăng ký bao gồm email và mật khẩu</param>
        /// <returns>AuthResponseDto chứa trạng thái đăng ký và thông tin người dùng</returns>
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "User",
                EmailConfirmed = false,
                VerificationToken = GenerateVerificationToken()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send verification email
            await SendVerificationEmail(user.Email, user.VerificationToken);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful. Please check your email for verification.",
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <summary>
        /// Đăng ký tài khoản admin mới với thông tin được cung cấp.
        /// Tài khoản admin được xác thực tự động và không yêu cầu xác nhận email.
        /// </summary>
        /// <param name="model">Thông tin đăng ký bao gồm email và mật khẩu</param>
        /// <returns>AuthResponseDto chứa trạng thái đăng ký và thông tin người dùng</returns>
        public async Task<AuthResponseDto> RegisterAdminAsync(RegisterDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "Admin",
                EmailConfirmed = true, // Admin accounts are automatically confirmed
                VerificationToken = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Admin registration successful",
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <summary>
        /// Gửi email xác thực đến người dùng với đường dẫn xác nhận.
        /// </summary>
        /// <param name="email">Địa chỉ email của người dùng</param>
        /// <param name="token">Token xác thực</param>
        private async Task SendVerificationEmail(string email, string token)
        {
            var confirmationLink = $"{_configuration["AppUrl"]}/api/Auth/verify-email?token={token}&email={Uri.EscapeDataString(email)}";
            var subject = "Verify Your Email";
            var body = $"Please click the link to verify your email: <a href='{confirmationLink}'>{confirmationLink}</a>";
            await _emailService.SendEmailAsync(email, subject, body);
        }

        /// <summary>
        /// Xác thực người dùng và tạo JWT token cùng refresh token khi đăng nhập thành công.
        /// 
        /// Quy trình xác thực:
        /// 1. Kiểm tra email tồn tại
        /// 2. Xác thực mật khẩu
        /// 3. Kiểm tra email đã được xác thực chưa
        /// 4. Tạo JWT token và refresh token
        /// 5. Lưu refresh token vào database
        /// </summary>
        /// <param name="model">Thông tin đăng nhập bao gồm email và mật khẩu</param>
        /// <returns>AuthResponseDto chứa token xác thực và thông tin người dùng</returns>
        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Please verify your email before logging in"
                };
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <summary>
        /// Đăng xuất người dùng bằng cách vô hiệu hóa refresh token.
        /// </summary>
        /// <param name="userId">ID của người dùng cần đăng xuất</param>
        /// <returns>Boolean cho biết trạng thái đăng xuất</returns>
        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.RefreshToken = null;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tạo mới JWT token và refresh token bằng cách sử dụng refresh token hiện có.
        /// 
        /// Quy trình refresh token:
        /// 1. Kiểm tra refresh token hợp lệ
        /// 2. Tạo JWT token mới
        /// 3. Tạo refresh token mới
        /// 4. Cập nhật refresh token trong database
        /// </summary>
        /// <param name="model">Thông tin refresh token</param>
        /// <returns>AuthResponseDto chứa token xác thực mới</returns>
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <summary>
        /// Khởi tạo quá trình đặt lại mật khẩu bằng cách tạo token reset và gửi email.
        /// </summary>
        /// <param name="model">Địa chỉ email cần đặt lại mật khẩu</param>
        /// <returns>AuthResponseDto cho biết trạng thái yêu cầu đặt lại mật khẩu</returns>
        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "If your email is registered, you will receive a password reset link"
                };
            }

            user.ResetPasswordToken = GenerateResetToken();
            await _context.SaveChangesAsync();

            // Send password reset email
            var resetLink = $"{_configuration["FrontendUrl"]}/reset-password?token={user.ResetPasswordToken}&email={Uri.EscapeDataString(user.Email)}";
            var subject = "Reset Your Password";
            var body = $"Please click the link to reset your password: <a href='{resetLink}'>{resetLink}</a>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return new AuthResponseDto
            {
                Success = true,
                Message = "If your email is registered, you will receive a password reset link"
            };
        }

        /// <summary>
        /// Đặt lại mật khẩu người dùng bằng token reset hợp lệ.
        /// </summary>
        /// <param name="model">Thông tin đặt lại mật khẩu bao gồm token và mật khẩu mới</param>
        /// <returns>AuthResponseDto cho biết trạng thái đặt lại mật khẩu</returns>
        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == model.Email && 
                u.ResetPasswordToken == model.Token);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid reset token or email"
                };
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            user.ResetPasswordToken = null;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully"
            };
        }

        /// <summary>
        /// Xác thực địa chỉ email người dùng bằng token xác thực.
        /// </summary>
        /// <param name="model">Thông tin xác thực email bao gồm token và email</param>
        /// <returns>AuthResponseDto cho biết trạng thái xác thực email</returns>
        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == model.Email && 
                u.VerificationToken == model.Token);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid verification token or email"
                };
            }

            user.EmailConfirmed = true;
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Email verified successfully",
                User = _mapper.Map<UserDto>(user)
            };
        }

        /// <summary>
        /// Thay đổi mật khẩu người dùng sau khi xác thực mật khẩu hiện tại.
        /// </summary>
        /// <param name="userId">ID của người dùng cần đổi mật khẩu</param>
        /// <param name="model">Thông tin thay đổi mật khẩu bao gồm mật khẩu hiện tại và mới</param>
        /// <returns>AuthResponseDto cho biết trạng thái thay đổi mật khẩu</returns>
        public async Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Verify current password
            if (!VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Current password is incorrect"
                };
            }

            // Update password
            user.PasswordHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }

        /// <summary>
        /// Tạo JWT token cho người dùng đã xác thực.
        /// 
        /// JWT Token bao gồm:
        /// - Claims: ID người dùng, email, và vai trò
        /// - Thời gian hết hạn: 1 giờ
        /// - Được ký bằng thuật toán HMAC-SHA256
        /// - Sử dụng secret key từ cấu hình
        /// 
        /// Cấu trúc JWT:
        /// - Header: Loại token và thuật toán ký
        /// - Payload: Claims của người dùng
        /// - Signature: Chữ ký xác thực token
        /// </summary>
        /// <param name="user">Thông tin người dùng để đưa vào token</param>
        /// <returns>Chuỗi JWT token</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo refresh token an toàn để duy trì phiên đăng nhập.
        /// Refresh token được tạo bằng cách sử dụng RandomNumberGenerator
        /// để đảm bảo tính ngẫu nhiên và bảo mật.
        /// </summary>
        /// <returns>Chuỗi refresh token</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Tạo token xác thực duy nhất cho việc xác thực email.
        /// Sử dụng GUID để đảm bảo tính duy nhất.
        /// </summary>
        /// <returns>Chuỗi token xác thực</returns>
        private string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Tạo token duy nhất cho chức năng đặt lại mật khẩu.
        /// Sử dụng GUID để đảm bảo tính duy nhất.
        /// </summary>
        /// <returns>Chuỗi token đặt lại</returns>
        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Mã hóa mật khẩu một cách an toàn bằng BCrypt.
        /// BCrypt tự động thêm salt và thực hiện nhiều vòng lặp để tăng tính bảo mật.
        /// </summary>
        /// <param name="password">Mật khẩu dạng văn bản cần mã hóa</param>
        /// <returns>Chuỗi mật khẩu đã mã hóa</returns>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Xác thực xem mật khẩu được cung cấp có khớp với hash đã lưu không.
        /// Sử dụng BCrypt để so sánh mật khẩu một cách an toàn.
        /// </summary>
        /// <param name="password">Mật khẩu dạng văn bản cần xác thực</param>
        /// <param name="hash">Hash mật khẩu đã lưu</param>
        /// <returns>Boolean cho biết mật khẩu có khớp với hash không</returns>
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
