using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface ICartItemService
{
    Task<Result<IEnumerable<CartItemDTO>>> GetAllAsync();
    Task<Result<CartItemDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<CartItemDTO>>> GetMyCartItemsAsync(Guid userId);
    Task<Result<CartItemDTO>> AddItemAsync(Guid userId, AddCartItemDTO request);
    Task<Result<CartItemDTO>> UpdateAsync(int id, Guid userId, UpdateCartItemDTO request);
    Task<Result> RemoveItemAsync(int id, Guid userId);
}
