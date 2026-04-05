using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IOrderItemRepository : IBaseRepository<OrderItem, int>
{
    Task<IEnumerable<OrderItem>> GetOrderItemsByOrderAsync(int orderId);
    Task<IEnumerable<OrderItem>> GetOrderItemsByVendorOfferAsync(int vendorOfferId);
}
