using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.VendorOffer;

public class UpdateVendorOfferDTO
{
    public decimal Price { get; set; }
    public VendorOfferStatus Status { get; set; }
    public int Quantity { get; set; }
}
