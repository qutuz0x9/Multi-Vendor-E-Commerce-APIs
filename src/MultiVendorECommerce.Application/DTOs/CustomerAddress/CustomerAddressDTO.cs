using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.CustomerAddress;

public class CustomerAddressDTO
{
    public int Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public CustomerAddressType AddressType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
