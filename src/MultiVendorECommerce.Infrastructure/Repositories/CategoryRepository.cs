using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MultiVendorECommerce.Infrastructure.Repositories;

public class CategoryRepository(ECommerceDbContext context) : BaseRepository<Category, int>(context), ICategoryRepository
{
    public override async Task DeleteAsync(Category entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<Category?> GetCategoryBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug);
    }
}
