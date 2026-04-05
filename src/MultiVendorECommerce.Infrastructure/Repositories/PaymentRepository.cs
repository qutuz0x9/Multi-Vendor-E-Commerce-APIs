using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment, int>, IPaymentRepository
{
    public PaymentRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetPaymentByOrderAsync(int orderId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(int status)
    {
        return await _dbSet.Where(p => (int)p.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByProviderAsync(string provider)
    {
        return await _dbSet.Where(p => p.Provider == provider).ToListAsync();
    }
}
