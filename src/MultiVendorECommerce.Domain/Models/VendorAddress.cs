using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Domain.Models;

public class VendorAddress
{
    public int Id { get; set; }
    public Guid VendorId { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public VendorAddressType AddressType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public Vendor Vendor { get; set; } = null!;
}
