namespace MultiVendorECommerce.Domain.Models;

public class OrderShippingAddress
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;

}
