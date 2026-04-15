using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDTO>();
        CreateMap<CreateCategoryDTO, Category>();
        CreateMap<UpdateCategoryDTO, Category>();
    }
}
