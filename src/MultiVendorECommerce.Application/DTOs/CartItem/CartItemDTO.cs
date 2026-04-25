namespace MultiVendorECommerce.Application.DTOs.CartItem;

public class CartItemDTO
{
    public int Id { get; set; }
    public int VendorOfferId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
