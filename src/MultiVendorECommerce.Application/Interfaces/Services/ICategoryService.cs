using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<CategoryDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<CategoryDTO>>> GetAllAsync();
    Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO request);
    Task<Result<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request);
    Task<Result> DeleteAsync(int id);
}
