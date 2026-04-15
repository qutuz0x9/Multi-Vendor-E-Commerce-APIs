using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IBrandService
{
    Task<Result<BrandDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<BrandDTO>>> GetAllAsync();
    Task<Result<BrandDTO>> CreateAsync(CreateBrandDTO request);
    Task<Result<BrandDTO>> UpdateAsync(int id, UpdateBrandDTO request);
    Task<Result> DeleteAsync(int id);
}
