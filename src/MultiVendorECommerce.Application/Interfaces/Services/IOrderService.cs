using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Result<OrderDTO>> CreateOrderAsync(Guid userId);
    Task<Result<IEnumerable<OrderDTO>>> GetAllOrdersAsync();
    Task<Result<OrderDTO>> GetOrderByIdAsync(int orderId);
    Task<Result<IEnumerable<OrderDTO>>> GetMyOrdersAsync(Guid userId);
    Task<Result> CancelOrderAsync(int orderId, Guid userId);
    Task<Result<OrderDTO>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDTO request);
}
