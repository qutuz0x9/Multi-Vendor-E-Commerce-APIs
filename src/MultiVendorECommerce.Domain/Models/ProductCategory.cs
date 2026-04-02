namespace MultiVendorECommerce.Domain.Models;

public class ProductCategory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
