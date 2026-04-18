namespace MultiVendorECommerce.Application.DTOs.ProductCategory;

public class ProductCategoryDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
