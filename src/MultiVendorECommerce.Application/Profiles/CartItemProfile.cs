using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class CartItemProfile : Profile
{
    public CartItemProfile()
    {
        CreateMap<CartItem, CartItemDTO>();
    }
}
