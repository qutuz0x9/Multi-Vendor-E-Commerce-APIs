using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.CustomerAddress;

public class UpdateCustomerAddressDTO
{
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public CustomerAddressType AddressType { get; set; }
}
