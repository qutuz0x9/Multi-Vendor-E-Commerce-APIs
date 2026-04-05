using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<Category, int>
{
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<Category?> GetCategoryBySlugAsync(string slug);
}
