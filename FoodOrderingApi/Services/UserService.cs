using AutoMapper;
using FoodOrderingApi.Data;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Dịch vụ xử lý thông tin người dùng
    /// 
    /// Tính năng:
    /// - Quản lý thông tin người dùng
    /// - Tìm kiếm người dùng theo ID và email
    /// - Cập nhật thông tin người dùng
    /// - Xóa tài khoản người dùng
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID
        /// </summary>
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo email
        /// </summary>
        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        public async Task<UserDto> UpdateUserAsync(int id, UserDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            _mapper.Map(userDto, user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Xóa tài khoản người dùng
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 