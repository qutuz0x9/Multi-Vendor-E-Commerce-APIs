using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Order;

public class UpdateOrderStatusDTO
{
    public OrderStatus Status { get; set; }
}
