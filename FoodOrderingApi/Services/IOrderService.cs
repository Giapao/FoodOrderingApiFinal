using FoodOrderingApi.DTOs;

namespace FoodOrderingApi.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrder(OrderDto orderDto);
        Task<OrderDto> CreateOrderFromCart(int cartId, string? specialInstructions, string phoneNumber, string deliveryAddress);
        Task<List<OrderDto>> GetOrdersByUser(int userId);
        Task<OrderDto> GetOrderById(int orderId);
        Task<OrderDto> UpdateOrderStatus(int orderId, string status, int userId);
        Task<OrderDto> CancelOrder(int orderId, string cancellationReason);
        Task<List<OrderDto>> GetOrdersByRestaurant(int restaurantId);
        Task<List<OrderDto>> GetOrdersByStatus(string status);
    }
}
