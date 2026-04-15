using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Brand;

public class BrandDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public BrandStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
