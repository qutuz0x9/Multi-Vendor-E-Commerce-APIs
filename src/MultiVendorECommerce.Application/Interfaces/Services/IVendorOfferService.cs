using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IVendorOfferService
{
    Task<Result<IEnumerable<VendorOfferDTO>>> GetAllAsync();
    Task<Result<VendorOfferDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByVendorAsync(Guid vendorId);
    Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByProductAsync(int productId);
    Task<Result<VendorOfferDTO>> CreateAsync(Guid vendorId, CreateVendorOfferDTO request);
    Task<Result<VendorOfferDTO>> UpdateAsync(Guid userId, int id, UpdateVendorOfferDTO request);
    Task<Result> DeleteAsync(int id);
}