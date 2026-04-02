using MultiVendorECommerce.Domain.Enums;
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Slug { get; set; }
    public CategoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

}
