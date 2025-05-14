using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? ResetPasswordToken { get; set; }
        public string? VerificationToken { get; set; } 
        public string? RefreshToken { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
