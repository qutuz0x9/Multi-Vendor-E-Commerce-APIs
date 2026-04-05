using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IVendorOfferRepository : IBaseRepository<VendorOffer, int>
{
    Task<VendorOffer?> GetOfferByVendorAndProductAsync(Guid vendorId, int productId);
    Task<IEnumerable<VendorOffer>> GetOffersByVendorAsync(Guid vendorId);
    Task<IEnumerable<VendorOffer>> GetOffersByProductAsync(int productId);
    Task<IEnumerable<VendorOffer>> GetOffersByStatusAsync(int status);
}
