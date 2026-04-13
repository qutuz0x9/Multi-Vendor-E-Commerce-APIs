using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Domain.Models;
namespace MultiVendorECommerce.Application.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RegisterUserDTO, User>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
        .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)).ReverseMap();

        CreateMap<User, AuthResponseDTO>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
        .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.Role, opt => opt.Ignore())
        .ForMember(dest => dest.Token, opt => opt.Ignore()).ReverseMap();

    }
}