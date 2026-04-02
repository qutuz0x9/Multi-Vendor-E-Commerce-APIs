using MultiVendorECommerce.Domain.Enums;
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    public BrandStatus Status { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
