using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class CustomerAddressProfile : Profile
{
    public CustomerAddressProfile()
    {
        CreateMap<CustomerAddress, CustomerAddressDTO>();
        CreateMap<CreateCustomerAddressDTO, CustomerAddress>();
        CreateMap<UpdateCustomerAddressDTO, CustomerAddress>();
    }
}
