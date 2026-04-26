using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.CustomerAddressServiceTest;

public class UpdateCustomerAddressTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICustomerAddressService _customerAddressService;

    public UpdateCustomerAddressTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);

        _customerAddressService = new CustomerAddressService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedAddress()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var address = new CustomerAddress
        {
            Id = 1,
            CustomerId = customerId,
            Address = "Old Address",
            City = "Old City",
            Country = "Egypt",
            PostalCode = "00000",
            AddressType = CustomerAddressType.Shipping,
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCustomerAddressDTO
        {
            Address = "New Address",
            City = "New City",
            Country = "Egypt",
            PostalCode = "99999",
            AddressType = CustomerAddressType.Billing
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Address.Should().Be(request.Address);
        result.Value.City.Should().Be(request.City);
        result.Value.PostalCode.Should().Be(request.PostalCode);
        result.Value.AddressType.Should().Be(request.AddressType);

        _customerAddressRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CustomerAddress>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAddressNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var request = new UpdateCustomerAddressDTO { Address = "X", City = "X", Country = "X", PostalCode = "X", AddressType = CustomerAddressType.Shipping };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CustomerAddress?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.UpdateAsync(99, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenAddressBelongsToAnotherCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var anotherCustomerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var address = new CustomerAddress
        {
            Id = 1,
            CustomerId = anotherCustomerId,
            Address = "Other St",
            City = "Cairo",
            Country = "Egypt",
            PostalCode = "11111",
            AddressType = CustomerAddressType.Shipping,
            CreatedAt = DateTime.UtcNow
        };
        var request = new UpdateCustomerAddressDTO { Address = "X", City = "X", Country = "X", PostalCode = "X", AddressType = CustomerAddressType.Shipping };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoCustomerProfile_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var request = new UpdateCustomerAddressDTO { Address = "X", City = "X", Country = "X", PostalCode = "X", AddressType = CustomerAddressType.Shipping };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}
