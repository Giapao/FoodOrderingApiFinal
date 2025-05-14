using AutoMapper;
using FoodOrderingApi.DTOs;
using FoodOrderingApi.DTOs.Auth;
using FoodOrderingApi.Models;

namespace FoodOrderingApi.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();
            CreateMap<LoginDto, User>();
        }
    }
} 