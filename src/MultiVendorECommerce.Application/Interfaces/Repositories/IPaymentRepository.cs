using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment, int>
{
    Task<Payment?> GetPaymentByOrderAsync(int orderId);
    Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(int status);
    Task<IEnumerable<Payment>> GetPaymentsByProviderAsync(string provider);
}
