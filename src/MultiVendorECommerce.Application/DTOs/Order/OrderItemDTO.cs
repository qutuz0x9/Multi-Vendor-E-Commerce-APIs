namespace MultiVendorECommerce.Application.DTOs.Order;

public class OrderItemDTO
{
    public int Id { get; set; }
    public int VendorOfferId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Price { get; set; }
}
