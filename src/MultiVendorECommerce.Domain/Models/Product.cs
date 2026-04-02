using MultiVendorECommerce.Domain.Enums;
using System.Text.Json;

namespace MultiVendorECommerce.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public JsonDocument? Feature { get; set; }
    public ProductStatus? Status { get; set; }
    public string Slug { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Brand Brand { get; set; } = null!;
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
