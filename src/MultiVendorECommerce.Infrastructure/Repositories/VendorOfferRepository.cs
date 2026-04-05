using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class VendorOfferRepository : BaseRepository<VendorOffer, int>, IVendorOfferRepository
{
    public VendorOfferRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<VendorOffer?> GetOfferByVendorAndProductAsync(Guid vendorId, int productId)
    {
        return await _dbSet.FirstOrDefaultAsync(vo => vo.VendorId == vendorId && vo.ProductId == productId);
    }

    public async Task<IEnumerable<VendorOffer>> GetOffersByVendorAsync(Guid vendorId)
    {
        return await _dbSet.Where(vo => vo.VendorId == vendorId).ToListAsync();
    }

    public async Task<IEnumerable<VendorOffer>> GetOffersByProductAsync(int productId)
    {
        return await _dbSet.Where(vo => vo.ProductId == productId).ToListAsync();
    }

    public async Task<IEnumerable<VendorOffer>> GetOffersByStatusAsync(int status)
    {
        return await _dbSet.Where(vo => (int)vo.Staus == status).ToListAsync();
    }
}
