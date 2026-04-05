using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class ProductCategoryRepository : BaseRepository<ProductCategory, int>, IProductCategoryRepository
{
    public ProductCategoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductCategory>> GetCategoriesByProductAsync(int productId)
    {
        return await _dbSet.Where(pc => pc.ProductId == productId).ToListAsync();
    }

    public async Task<IEnumerable<ProductCategory>> GetProductsByCategory(int categoryId)
    {
        return await _dbSet.Where(pc => pc.CategoryId == categoryId).ToListAsync();
    }
}
