using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface ICartSessionRepository : IBaseRepository<CartSession, Guid>
{
    Task<CartSession?> GetCartByCustomerAsync(Guid customerId);
    Task<CartSession?> GetCartWithItemsAsync(Guid cartId);
}
