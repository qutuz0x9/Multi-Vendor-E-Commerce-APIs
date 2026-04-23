using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.VendorOffer;

public class VendorOfferDTO
{
    public int Id { get; set; }
    public Guid VendorId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public VendorOfferStatus Status { get; set; }
}
