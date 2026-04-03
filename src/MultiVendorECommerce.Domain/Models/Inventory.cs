using MultiVendorECommerce.Domain.Enums;
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class Inventory
{
    public int Id { get; set; }
    public int VendorOfferId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public InventoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public VendorOffer VendorOffer { get; set; } = null!;
}
