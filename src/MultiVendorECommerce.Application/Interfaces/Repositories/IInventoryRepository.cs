using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IInventoryRepository : IBaseRepository<Inventory, int>
{
    Task<Inventory?> GetInventoryByVendorOfferAsync(int vendorOfferId);
    Task<IEnumerable<Inventory>> GetLowStockInventoriesAsync(int threshold);
    Task<int> GetTotalQuantityByVendorOfferAsync(int vendorOfferId);
}
