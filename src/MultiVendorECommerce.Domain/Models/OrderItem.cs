namespace MultiVendorECommerce.Domain.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int VendorOfferId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public VendorOffer VendorOffer { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
