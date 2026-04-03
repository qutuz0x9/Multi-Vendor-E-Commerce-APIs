using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Domain.Models;

public class VendorOffer
{
    public int Id { get; set; }
    public Guid VendorId { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public VendorOfferStatus Staus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Vendor? Vendor { get; set; } = null!;
    public Product? Product { get; set; } = null!;
    public Inventory Inventory { get; set; } = null!;
}
