using System.Text.Json;
using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDTO>()
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
            .ForMember(dest => dest.Feature, opt => opt.MapFrom(src => src.Feature != null ? src.Feature.RootElement : (JsonElement?)null))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ProductCategories));

        CreateMap<CreateProductDTO, Product>()
            .ForMember(dest => dest.Feature, opt => opt.MapFrom(src => src.Feature.HasValue ? JsonDocument.Parse(src.Feature.Value.GetRawText()) : null));

        CreateMap<UpdateProductDTO, Product>()
            .ForMember(dest => dest.Feature, opt => opt.MapFrom(src => src.Feature.HasValue ? JsonDocument.Parse(src.Feature.Value.GetRawText()) : null));
    }
}
