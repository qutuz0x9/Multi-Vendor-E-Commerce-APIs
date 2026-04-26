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

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.CustomerAddressServiceTest;

public class GetMyAddressesTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CustomerAddressService>> _loggerMock;
    private readonly ICustomerAddressService _customerAddressService;

    public GetMyAddressesTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<CustomerAddressService>>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);

        _customerAddressService = new CustomerAddressService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMyAddressesAsync_WhenCustomerHasAddresses_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var addresses = new List<CustomerAddress>
        {
            new() { Id = 1, CustomerId = customerId, Address = "123 Main St", City = "Cairo", Country = "Egypt", PostalCode = "12345", AddressType = CustomerAddressType.Shipping, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, CustomerId = customerId, Address = "456 Billing Ave", City = "Giza", Country = "Egypt", PostalCode = "67890", AddressType = CustomerAddressType.Billing, CreatedAt = DateTime.UtcNow }
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetAddressesByCustomerAsync(customerId)).ReturnsAsync(addresses);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetMyAddressesAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyAddressesAsync_WhenCustomerHasNoAddresses_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _customerAddressRepositoryMock.Setup(r => r.GetAddressesByCustomerAsync(customerId)).ReturnsAsync(new List<CustomerAddress>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetMyAddressesAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyAddressesAsync_WhenNoCustomerProfile_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetMyAddressesAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}
