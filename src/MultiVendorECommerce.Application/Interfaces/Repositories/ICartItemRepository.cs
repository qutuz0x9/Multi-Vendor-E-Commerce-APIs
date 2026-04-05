using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface ICartItemRepository : IBaseRepository<CartItem, int>
{
    Task<IEnumerable<CartItem>> GetItemsByCartAsync(Guid cartSessionId);
    Task<CartItem?> GetCartItemByVendorOfferAsync(Guid cartSessionId, int vendorOfferId);
}
