using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class VendorAddressRepository : BaseRepository<VendorAddress, int>, IVendorAddressRepository
{
    public VendorAddressRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VendorAddress>> GetAddressesByVendorAsync(Guid vendorId)
    {
        return await _dbSet.Where(va => va.VendorId == vendorId).ToListAsync();
    }

    public async Task<IEnumerable<VendorAddress>> GetAddressesByTypeAsync(Guid vendorId, int addressType)
    {
        return await _dbSet.Where(va => va.VendorId == vendorId && (int)va.AddressType == addressType).ToListAsync();
    }
}
