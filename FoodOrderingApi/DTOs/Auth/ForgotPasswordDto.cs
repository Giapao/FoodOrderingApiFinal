using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 