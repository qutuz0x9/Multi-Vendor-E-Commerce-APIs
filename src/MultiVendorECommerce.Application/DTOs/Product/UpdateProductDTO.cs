using System.Text.Json;
using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Product;

public class UpdateProductDTO
{
    public int BrandId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public JsonElement? Feature { get; set; }
    public ProductStatus Status { get; set; }
    public List<int> CategoryIds { get; set; } = new();
}
