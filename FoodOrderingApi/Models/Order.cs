using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } 
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [StringLength(200)]
        public string DeliveryAddress { get; set; }
        
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
        
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PreparedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        [StringLength(200)]
        public string? CancellationReason { get; set; }
        
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
