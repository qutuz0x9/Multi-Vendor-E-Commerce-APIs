using MultiVendorECommerce.Application.DTOs.CartItem;

namespace MultiVendorECommerce.Application.DTOs.CartSession;

public class CartSessionDTO
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
}
