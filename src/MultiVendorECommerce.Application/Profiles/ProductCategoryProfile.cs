using AutoMapper;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<ProductCategory, ProductCategoryDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
        CreateMap<CreateProductCategoryDTO, ProductCategory>();
    }
}
