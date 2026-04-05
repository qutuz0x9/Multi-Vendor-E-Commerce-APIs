using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class InventoryRepository : BaseRepository<Inventory, int>, IInventoryRepository
{
    public InventoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetInventoryByVendorOfferAsync(int vendorOfferId)
    {
        return await _dbSet.FirstOrDefaultAsync(i => i.VendorOfferId == vendorOfferId);
    }

    public async Task<IEnumerable<Inventory>> GetLowStockInventoriesAsync(int threshold)
    {
        return await _dbSet.Where(i => i.Quantity <= threshold).ToListAsync();
    }

    public async Task<int> GetTotalQuantityByVendorOfferAsync(int vendorOfferId)
    {
        var inventory = await _dbSet.FirstOrDefaultAsync(i => i.VendorOfferId == vendorOfferId);
        return inventory?.Quantity ?? 0;
    }
}
