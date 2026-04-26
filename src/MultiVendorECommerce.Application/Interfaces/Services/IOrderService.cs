using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDTO>> CreateOrderAsync(Guid userId);
}
