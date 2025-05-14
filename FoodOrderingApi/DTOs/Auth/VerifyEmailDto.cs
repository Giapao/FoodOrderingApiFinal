using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 