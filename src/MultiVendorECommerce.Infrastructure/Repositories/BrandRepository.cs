using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class BrandRepository : BaseRepository<Brand, int>, IBrandRepository
{
    public BrandRepository(ECommerceDbContext context) : base(context)
    {
    }

    public override async Task DeleteAsync(Brand entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Brand?> GetBrandByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
    }

    public async Task<Brand?> GetBrandBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Slug == slug);
    }
}
