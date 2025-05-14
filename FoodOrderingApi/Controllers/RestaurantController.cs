using FoodOrderingApi.Models;
using FoodOrderingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Hiển thị danh sách nhà hàng (có phân trang)
        /// </summary>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <returns>View danh sách nhà hàng</returns>
        public async Task<IActionResult> Index(int page = 1)
        {
            var restaurants = await _restaurantService.GetRestaurantsAsync(page);
            return View(restaurants);
        }

        /// <summary>
        /// Hiển thị chi tiết một nhà hàng và danh sách món ăn của nhà hàng đó (có phân trang)
        /// </summary>
        /// <param name="id">Id nhà hàng</param>
        /// <param name="page">Trang hiện tại của menu (mặc định 1)</param>
        /// <returns>View chi tiết nhà hàng và menu</returns>
        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            var menuItems = await _restaurantService.GetMenuItemsAsync(id, page);
            ViewBag.MenuItems = menuItems;
            return View(restaurant);
        }
    }
} 