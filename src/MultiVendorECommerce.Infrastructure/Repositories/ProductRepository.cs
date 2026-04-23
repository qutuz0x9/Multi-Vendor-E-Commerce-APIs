using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product, int>, IProductRepository
{
    public ProductRepository(ECommerceDbContext context) : base(context)
    {
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .ToListAsync();
    }

    public override async Task DeleteAsync(Product entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.BrandId == brandId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
            .ToListAsync();
    }

    public async Task<Product?> GetProductBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }
}
