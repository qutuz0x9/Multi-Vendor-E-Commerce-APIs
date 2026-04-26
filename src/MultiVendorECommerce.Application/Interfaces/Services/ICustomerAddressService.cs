using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface ICustomerAddressService
{
    // Admin operations
    Task<Result<CustomerAddressDTO>> GetByIdAsync(int id);
    Task<Result<IEnumerable<CustomerAddressDTO>>> GetAllAsync();
    Task<Result<IEnumerable<CustomerAddressDTO>>> GetByTypeAsync(CustomerAddressType addressType);

    // Customer operations
    Task<Result<IEnumerable<CustomerAddressDTO>>> GetMyAddressesAsync(Guid userId);
    Task<Result<CustomerAddressDTO>> CreateAsync(Guid userId, CreateCustomerAddressDTO request);
    Task<Result<CustomerAddressDTO>> UpdateAsync(int id, Guid userId, UpdateCustomerAddressDTO request);
    Task<Result> DeleteAsync(int id, Guid userId);
}
