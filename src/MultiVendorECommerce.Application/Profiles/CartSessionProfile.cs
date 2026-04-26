using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CartSession;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class CartSessionProfile : Profile
{
    public CartSessionProfile()
    {
        CreateMap<CartSession, CartSessionDTO>();
    }
}
