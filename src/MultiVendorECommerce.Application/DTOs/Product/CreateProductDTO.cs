using System.Text.Json;

namespace MultiVendorECommerce.Application.DTOs.Product;

public class CreateProductDTO
{
    public int BrandId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public JsonElement? Feature { get; set; }
    public List<int> CategoryIds { get; set; } = new();
}
