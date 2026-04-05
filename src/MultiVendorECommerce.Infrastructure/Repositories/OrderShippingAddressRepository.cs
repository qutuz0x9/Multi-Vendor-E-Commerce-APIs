using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class OrderShippingAddressRepository : BaseRepository<OrderShippingAddress, int>, IOrderShippingAddressRepository
{
    public OrderShippingAddressRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<OrderShippingAddress?> GetAddressByOrderAsync(int orderId)
    {
        return await _dbSet.FirstOrDefaultAsync(osa => osa.OrderId == orderId);
    }
}
