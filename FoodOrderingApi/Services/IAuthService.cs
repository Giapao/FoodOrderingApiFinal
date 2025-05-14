using FoodOrderingApi.DTOs.Auth;

namespace FoodOrderingApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> RegisterAdminAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<bool> LogoutAsync(int userId);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto model);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto model);
        Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto model);
    }
}
