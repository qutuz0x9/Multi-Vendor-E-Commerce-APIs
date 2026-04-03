using MultiVendorECommerce.Domain.Enums;
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class Order
{
    public int Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment Payment { get; set; } = null!;
    public OrderShippingAddress ShippingAddress { get; set; } = null!;

}
