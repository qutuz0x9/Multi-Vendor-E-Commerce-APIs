using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Order;

public class OrderDTO
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
    public OrderShippingAddressDTO ShippingAddress { get; set; } = null!;
}
