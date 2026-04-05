using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer, Guid>
{
    Task<Customer?> GetCustomerByUserIdAsync(Guid userId);
    Task<IEnumerable<Customer>> GetDeletedCustomersAsync();
}
