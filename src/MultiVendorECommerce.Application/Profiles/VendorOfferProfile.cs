using AutoMapper;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Profiles;

public class VendorOfferProfile : Profile
{
    public VendorOfferProfile()
    {
        CreateMap<VendorOffer, VendorOfferDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Staus))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<CreateVendorOfferDTO, VendorOffer>()
            .ForMember(dest => dest.Staus, opt => opt.Ignore());
            

        CreateMap<UpdateVendorOfferDTO, VendorOffer>()
            .ForMember(dest => dest.Staus, opt => opt.MapFrom(src => src.Status));
            
    }
}
