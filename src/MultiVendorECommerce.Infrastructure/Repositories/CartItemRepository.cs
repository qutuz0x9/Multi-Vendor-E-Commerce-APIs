using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class CartItemRepository : BaseRepository<CartItem, int>, ICartItemRepository
{
    public CartItemRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetItemsByCartAsync(Guid cartSessionId)
    {
        return await _dbSet.Where(ci => ci.CartSessionId == cartSessionId).ToListAsync();
    }

    public async Task<CartItem?> GetCartItemByVendorOfferAsync(Guid cartSessionId, int vendorOfferId)
    {
        return await _dbSet.FirstOrDefaultAsync(ci => ci.CartSessionId == cartSessionId && ci.VendorOfferId == vendorOfferId);
    }
}
