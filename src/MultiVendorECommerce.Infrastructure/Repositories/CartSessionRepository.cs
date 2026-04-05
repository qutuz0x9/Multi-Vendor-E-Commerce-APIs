using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class CartSessionRepository : BaseRepository<CartSession, Guid>, ICartSessionRepository
{
    public CartSessionRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<CartSession?> GetCartByCustomerAsync(Guid customerId)
    {
        return await _dbSet.FirstOrDefaultAsync(cs => cs.CustomerId == customerId);
    }

    public async Task<CartSession?> GetCartWithItemsAsync(Guid cartId)
    {
        return await _dbSet.Include(cs => cs.CartItems).FirstOrDefaultAsync(cs => cs.Id == cartId);
    }
}
