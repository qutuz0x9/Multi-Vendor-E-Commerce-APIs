using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IBrandRepository : IBaseRepository<Brand, int>
{
    Task<Brand?> GetBrandByNameAsync(string name);
    Task<Brand?> GetBrandBySlugAsync(string slug);
}
