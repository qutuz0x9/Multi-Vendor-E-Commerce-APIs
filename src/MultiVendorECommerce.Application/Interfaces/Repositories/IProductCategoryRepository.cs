using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IProductCategoryRepository : IBaseRepository<ProductCategory, int>
{
    Task<IEnumerable<ProductCategory>> GetCategoriesByProductAsync(int productId);
    Task<IEnumerable<ProductCategory>> GetProductsByCategory(int categoryId);
}
