using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class CustomerRepository : BaseRepository<Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetCustomerByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Customer>> GetDeletedCustomersAsync()
    {
        return await _dbSet.IgnoreQueryFilters().Where(c => c.IsDeleted).ToListAsync();
    }
}
