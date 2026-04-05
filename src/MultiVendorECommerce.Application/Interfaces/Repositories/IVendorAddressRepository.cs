using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IVendorAddressRepository : IBaseRepository<VendorAddress, int>
{
    Task<IEnumerable<VendorAddress>> GetAddressesByVendorAsync(Guid vendorId);
    Task<IEnumerable<VendorAddress>> GetAddressesByTypeAsync(Guid vendorId, int addressType);
}
