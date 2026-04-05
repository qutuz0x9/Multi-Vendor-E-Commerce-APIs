using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Interfaces.Repositories;

public interface IOrderShippingAddressRepository : IBaseRepository<OrderShippingAddress, int>
{
    Task<OrderShippingAddress?> GetAddressByOrderAsync(int orderId);
}
