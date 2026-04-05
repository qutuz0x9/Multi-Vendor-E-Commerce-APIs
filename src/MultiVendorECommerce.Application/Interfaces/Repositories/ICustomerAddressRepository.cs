using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface ICustomerAddressRepository : IBaseRepository<CustomerAddress, int>
{
    Task<IEnumerable<CustomerAddress>> GetAddressesByCustomerAsync(Guid customerId);
    Task<IEnumerable<CustomerAddress>> GetAddressesByTypeAsync(Guid customerId, int addressType);
}
