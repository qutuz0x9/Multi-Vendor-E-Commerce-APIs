using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IBaseRepository<Order, int>
{
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(int status);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
}
