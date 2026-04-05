using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class OrderItemRepository : BaseRepository<OrderItem, int>, IOrderItemRepository
{
    public OrderItemRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderAsync(int orderId)
    {
        return await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync();
    }

    public async Task<IEnumerable<OrderItem>> GetOrderItemsByVendorOfferAsync(int vendorOfferId)
    {
        return await _dbSet.Where(oi => oi.VendorOfferId == vendorOfferId).ToListAsync();
    }
}
