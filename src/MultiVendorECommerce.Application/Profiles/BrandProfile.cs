using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandDTO>();
        CreateMap<CreateBrandDTO, Brand>();
        CreateMap<UpdateBrandDTO, Brand>();
    }
}
