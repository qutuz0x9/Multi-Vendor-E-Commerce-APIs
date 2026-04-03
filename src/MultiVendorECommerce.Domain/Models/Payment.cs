

using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Domain.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Provider { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }


    // Navigation properties
    public Order Order { get; set; } = null!;

}
