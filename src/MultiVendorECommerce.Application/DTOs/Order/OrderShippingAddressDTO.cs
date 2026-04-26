namespace MultiVendorECommerce.Application.DTOs.Order;

public class OrderShippingAddressDTO
{
    public int Id { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;
}
