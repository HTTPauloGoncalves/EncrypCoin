using AutoMapper;
using EncrypCoin.API.Dtos.Application.User.Response;
using EncrypCoin.API.Models;
using EncrypCoin.API.Dtos.Application.User.Request;

namespace EncrypCoin.API.Dtos.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name));

            CreateMap<User, UserLoginResponseDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name));

            CreateMap<UserRegisterRequestDto, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }

}

