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

public class DeleteCustomerAddressTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICustomerAddressService _customerAddressService;

    public DeleteCustomerAddressTest()
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
    public async Task DeleteAsync_WhenAddressExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var address = new CustomerAddress
        {
            Id = 1,
            CustomerId = customerId,
            Address = "123 Main St",
            City = "Cairo",
            Country = "Egypt",
            PostalCode = "12345",
            AddressType = CustomerAddressType.Shipping,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.DeleteAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);

        _customerAddressRepositoryMock.Verify(r => r.DeleteAsync(address), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAddressNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CustomerAddress?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.DeleteAsync(99, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _customerAddressRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CustomerAddress>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenAddressBelongsToAnotherCustomer_ShouldReturnForbidden()
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
            Address = "456 Other St",
            City = "Alexandria",
            Country = "Egypt",
            PostalCode = "99999",
            AddressType = CustomerAddressType.Billing,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.DeleteAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);

        _customerAddressRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CustomerAddress>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNoCustomerProfile_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.DeleteAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}
