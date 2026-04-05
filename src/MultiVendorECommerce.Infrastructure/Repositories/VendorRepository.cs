using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class VendorRepository : BaseRepository<Vendor, Guid>, IVendorRepository
{
    public VendorRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Vendor?> GetVendorByBusinessNameAsync(string businessName)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.BusinessName == businessName);
    }

    public async Task<Vendor?> GetVendorBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.Slug == slug);
    }

    public async Task<Vendor?> GetVendorByWebsiteUrlAsync(string websiteUrl)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.WebsiteUrl == websiteUrl);
    }

    public async Task<IEnumerable<Vendor>> GetVendorsByStatusAsync(int status)
    {
        return await _dbSet.Where(v => (int)v.Status == status).ToListAsync();
    }
}
