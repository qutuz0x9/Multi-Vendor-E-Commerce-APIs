using MultiVendorECommerce.Application.DTOs.CartSession;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface ICartSessionService
{
    Task<Result<IEnumerable<CartSessionDTO>>> GetAllAsync();
    Task<Result<CartSessionDTO>> GetByIdAsync(Guid id);
    Task<Result<CartSessionDTO>> GetMyCartAsync(Guid userId);
    Task<Result<CartSessionDTO>> CreateAsync(Guid userId);
    Task<Result> DeleteAsync(Guid userId);
}
