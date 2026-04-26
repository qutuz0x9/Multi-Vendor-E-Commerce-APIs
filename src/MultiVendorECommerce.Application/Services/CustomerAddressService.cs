using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class CustomerAddressService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<CustomerAddressService> logger) : ICustomerAddressService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<CustomerAddressService> _logger = logger;

    public async Task<Result<CustomerAddressDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching customer address {AddressId}", id);
        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
        {
            _logger.LogWarning("Address {AddressId} not found", id);
            return Result<CustomerAddressDTO>.Failure(Error.NotFound("Address not found."), 404);
        }

        return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all customer addresses");
        var addresses = await _unitOfWork.CustomerAddresses.GetAllAsync();
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetByTypeAsync(CustomerAddressType addressType)
    {
        _logger.LogDebug("Fetching customer addresses by type {AddressType}", addressType);
        var addresses = await _unitOfWork.CustomerAddresses.FindAsync(a => a.AddressType == addressType);
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetMyAddressesAsync(Guid userId)
    {
        _logger.LogDebug("Fetching addresses for user {UserId}", userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Get addresses failed: no customer profile for user {UserId}", userId);
            return Result<IEnumerable<CustomerAddressDTO>>.Failure(Error.Forbidden("Only customers can access addresses."), 403);
        }

        var addresses = await _unitOfWork.CustomerAddresses.GetAddressesByCustomerAsync(customer.Id);
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<CustomerAddressDTO>> CreateAsync(Guid userId, CreateCustomerAddressDTO request)
    {
        _logger.LogInformation("Creating address for user {UserId}", userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Address creation failed: no customer profile for user {UserId}", userId);
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("Only customers can add addresses."), 403);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var address = new CustomerAddress
            {
                CustomerId = customer.Id,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PostalCode = request.PostalCode,
                AddressType = request.AddressType,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CustomerAddresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Address {AddressId} created for customer {CustomerId}", address.Id, customer.Id);
            return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address), 201);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogWarning("Address creation failed for customer {CustomerId} due to an unexpected error", customer.Id);
            return Result<CustomerAddressDTO>.Failure(Error.Failure(), 500);
        }

    }

    public async Task<Result<CustomerAddressDTO>> UpdateAsync(int id, Guid userId, UpdateCustomerAddressDTO request)
    {
        _logger.LogInformation("Updating address {AddressId} for user {UserId}", id, userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Address update failed: no customer profile for user {UserId}", userId);
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("Only customers can update addresses."), 403);
        }

        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
        {
            _logger.LogWarning("Address update failed: address {AddressId} not found", id);
            return Result<CustomerAddressDTO>.Failure(Error.NotFound("Address not found."), 404);
        }

        if (address.CustomerId != customer.Id)
        {
            _logger.LogWarning("Address update forbidden: address {AddressId} does not belong to customer {CustomerId}", id, customer.Id);
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("You do not have access to this address."), 403);
        }

        address.Address = request.Address;
        address.City = request.City;
        address.Country = request.Country;
        address.PostalCode = request.PostalCode;
        address.AddressType = request.AddressType;
        address.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.CustomerAddresses.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address {AddressId} updated successfully", id);
        return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address));
    }

    public async Task<Result> DeleteAsync(int id, Guid userId)
    {
        _logger.LogInformation("Deleting address {AddressId} for user {UserId}", id, userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Address delete failed: no customer profile for user {UserId}", userId);
            return Result.Failure(Error.Forbidden("Only customers can delete addresses."), 403);
        }

        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
        {
            _logger.LogWarning("Address delete failed: address {AddressId} not found", id);
            return Result.Failure(Error.NotFound("Address not found."), 404);
        }

        if (address.CustomerId != customer.Id)
        {
            _logger.LogWarning("Address delete forbidden: address {AddressId} does not belong to customer {CustomerId}", id, customer.Id);
            return Result.Failure(Error.Forbidden("You do not have access to this address."), 403);
        }

        await _unitOfWork.CustomerAddresses.DeleteAsync(address);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Address {AddressId} deleted successfully", id);
        return Result.Success();
    }
}
