using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class CustomerAddressRepository : BaseRepository<CustomerAddress, int>, ICustomerAddressRepository
{
    public CustomerAddressRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CustomerAddress>> GetAddressesByCustomerAsync(Guid customerId)
    {
        return await _dbSet.Where(ca => ca.CustomerId == customerId).ToListAsync();
    }

    public async Task<IEnumerable<CustomerAddress>> GetAddressesByTypeAsync(Guid customerId, int addressType)
    {
        return await _dbSet.Where(ca => ca.CustomerId == customerId && (int)ca.AddressType == addressType).ToListAsync();
    }
}
