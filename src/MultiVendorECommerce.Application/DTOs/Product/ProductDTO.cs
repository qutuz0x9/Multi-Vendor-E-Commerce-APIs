using System.Text.Json;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Application.DTOs.Product;

public class ProductDTO
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public JsonElement? Feature { get; set; }
    public string Slug { get; set; } = null!;
    public ProductStatus? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public IEnumerable<ProductCategoryDTO> Categories { get; set; } = new List<ProductCategoryDTO>();
}
