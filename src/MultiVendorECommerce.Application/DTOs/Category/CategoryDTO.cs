using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Category;

public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Slug { get; set; }
    public CategoryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
