namespace MultiVendorECommerce.Application.DTOs.VendorOffer;

public class CreateVendorOfferDTO
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
