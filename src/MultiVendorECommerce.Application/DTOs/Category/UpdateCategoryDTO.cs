using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Category;

public class UpdateCategoryDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public CategoryStatus Status { get; set; }
}
