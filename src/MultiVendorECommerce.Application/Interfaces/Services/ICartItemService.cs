using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface ICartItemService
{
    Task<Result<CartItemDTO>> AddItemAsync(Guid userId, AddCartItemDTO request);
}
