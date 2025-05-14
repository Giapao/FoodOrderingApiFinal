using FoodOrderingApi.Models;

namespace FoodOrderingApi.Services
{
    public interface IRestaurantService
    {
        Task<PagedResult<Restaurant>> GetRestaurantsAsync(int pageNumber = 1, int pageSize = 9);
        Task<Restaurant> GetRestaurantByIdAsync(int id);
        Task<PagedResult<MenuItem>> GetMenuItemsAsync(int restaurantId, int pageNumber = 1, int pageSize = 12);
        Task<MenuItem> GetMenuItemByIdAsync(int id);
        Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem);
        Task<MenuItem> UpdateMenuItemAsync(int id, MenuItem menuItem);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<IEnumerable<MenuItem>> SearchMenuItemsAsync(int restaurantId, string searchTerm);
        Task<IEnumerable<Restaurant>> SearchRestaurantsAsync(string searchTerm);
    }
} 