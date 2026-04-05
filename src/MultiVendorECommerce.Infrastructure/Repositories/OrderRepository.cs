using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class OrderRepository : BaseRepository<Order, int>, IOrderRepository
{
    public OrderRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(int status)
    {
        return await _dbSet.Where(o => (int)o.Status == status).ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await _dbSet.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
