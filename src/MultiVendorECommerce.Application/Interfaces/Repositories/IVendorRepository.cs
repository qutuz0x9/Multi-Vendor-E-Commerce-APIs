using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IVendorRepository : IBaseRepository<Vendor, Guid>
{
    Task<Vendor?> GetVendorByIdAsync(Guid id);
    Task<Vendor?> GetVendorByBusinessNameAsync(string businessName);
    Task<Vendor?> GetVendorBySlugAsync(string slug);
    Task<Vendor?> GetVendorByWebsiteUrlAsync(string websiteUrl);
    Task<IEnumerable<Vendor>> GetVendorsByStatusAsync(int status);
}
