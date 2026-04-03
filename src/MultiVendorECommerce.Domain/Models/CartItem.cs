
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public Guid CartSessionId { get; set; }
    public int VendorOfferId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public CartSession CartSession { get; set; } = null!;
    public VendorOffer VendorOffer { get; set; } = null!;
}
