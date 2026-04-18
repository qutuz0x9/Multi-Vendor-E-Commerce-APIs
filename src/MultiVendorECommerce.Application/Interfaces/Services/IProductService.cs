using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<Result<ProductDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<ProductDTO>>> GetAllAsync();
    Task<Result<IEnumerable<ProductDTO>>> GetProductsByBrandAsync(int brandId);
    Task<Result<IEnumerable<ProductDTO>>> GetProductsByCategoryAsync(int categoryId);
    Task<Result<ProductDTO>> CreateAsync(CreateProductDTO request);
    Task<Result<ProductDTO>> UpdateAsync(int id, UpdateProductDTO request);
    Task<Result> DeleteAsync(int id);
}
