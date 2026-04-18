using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IProductCategoryService
{
    Task<Result<IEnumerable<ProductCategoryDTO>>> GetCategoriesByProductAsync(int productId);
    Task<Result<IEnumerable<ProductCategoryDTO>>> GetProductsByCategoryAsync(int categoryId);
    Task<Result<ProductCategoryDTO>> AddProductToCategoryAsync(CreateProductCategoryDTO request);
    Task<Result> RemoveProductFromCategoryAsync(int id);
}
