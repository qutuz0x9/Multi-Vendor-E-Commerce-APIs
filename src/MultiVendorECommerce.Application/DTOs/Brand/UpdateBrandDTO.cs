using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Brand;

public class UpdateBrandDTO
{
    public string Name { get; set; } = null!;
    public BrandStatus Status { get; set; }
}
