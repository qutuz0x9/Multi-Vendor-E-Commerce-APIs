using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class CustomerAddressService(IUnitOfWork unitOfWork, IMapper mapper) : ICustomerAddressService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CustomerAddressDTO>> GetByIdAsync(int id)
    {
        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
            return Result<CustomerAddressDTO>.Failure(Error.NotFound("Address not found."), 404);

        return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetAllAsync()
    {
        var addresses = await _unitOfWork.CustomerAddresses.GetAllAsync();
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetByTypeAsync(CustomerAddressType addressType)
    {
        var addresses = await _unitOfWork.CustomerAddresses.FindAsync(a => a.AddressType == addressType);
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<IEnumerable<CustomerAddressDTO>>> GetMyAddressesAsync(Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<IEnumerable<CustomerAddressDTO>>.Failure(Error.Forbidden("Only customers can access addresses."), 403);

        var addresses = await _unitOfWork.CustomerAddresses.GetAddressesByCustomerAsync(customer.Id);
        return Result<IEnumerable<CustomerAddressDTO>>.Success(_mapper.Map<IEnumerable<CustomerAddressDTO>>(addresses));
    }

    public async Task<Result<CustomerAddressDTO>> CreateAsync(Guid userId, CreateCustomerAddressDTO request)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("Only customers can add addresses."), 403);

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

            return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address), 201);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<CustomerAddressDTO>.Failure(Error.Failure(), 500);
        }

    }

    public async Task<Result<CustomerAddressDTO>> UpdateAsync(int id, Guid userId, UpdateCustomerAddressDTO request)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("Only customers can update addresses."), 403);

        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
            return Result<CustomerAddressDTO>.Failure(Error.NotFound("Address not found."), 404);

        if (address.CustomerId != customer.Id)
            return Result<CustomerAddressDTO>.Failure(Error.Forbidden("You do not have access to this address."), 403);

        address.Address = request.Address;
        address.City = request.City;
        address.Country = request.Country;
        address.PostalCode = request.PostalCode;
        address.AddressType = request.AddressType;
        address.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.CustomerAddresses.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return Result<CustomerAddressDTO>.Success(_mapper.Map<CustomerAddressDTO>(address));
    }

    public async Task<Result> DeleteAsync(int id, Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result.Failure(Error.Forbidden("Only customers can delete addresses."), 403);

        var address = await _unitOfWork.CustomerAddresses.GetByIdAsync(id);
        if (address is null)
            return Result.Failure(Error.NotFound("Address not found."), 404);

        if (address.CustomerId != customer.Id)
            return Result.Failure(Error.Forbidden("You do not have access to this address."), 403);

        await _unitOfWork.CustomerAddresses.DeleteAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
