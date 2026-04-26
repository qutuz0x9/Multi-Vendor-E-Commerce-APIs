using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDTO>();
        CreateMap<OrderItem, OrderItemDTO>();
        CreateMap<OrderShippingAddress, OrderShippingAddressDTO>();
    }
}
