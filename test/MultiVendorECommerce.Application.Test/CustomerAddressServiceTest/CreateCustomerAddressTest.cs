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

public class CreateCustomerAddressTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CustomerAddressService>> _loggerMock;
    private readonly ICustomerAddressService _customerAddressService;

    public CreateCustomerAddressTest()
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
    public async Task CreateAsync_WithValidData_ShouldReturnCreatedAddress()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var request = new CreateCustomerAddressDTO
        {
            Address = "123 Main St",
            City = "Cairo",
            Country = "Egypt",
            PostalCode = "12345",
            AddressType = CustomerAddressType.Shipping
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _customerAddressRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerAddress>()))
            .ReturnsAsync((CustomerAddress a) => a);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.CreateAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Address.Should().Be(request.Address);
        result.Value.City.Should().Be(request.City);
        result.Value.Country.Should().Be(request.Country);
        result.Value.PostalCode.Should().Be(request.PostalCode);
        result.Value.AddressType.Should().Be(request.AddressType);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _customerAddressRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CustomerAddress>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSaveChangesFails_ShouldRollbackAndReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customer = new Customer { Id = customerId, UserId = userId };
        var request = new CreateCustomerAddressDTO
        {
            Address = "123 Main St",
            City = "Cairo",
            Country = "Egypt",
            PostalCode = "12345",
            AddressType = CustomerAddressType.Shipping
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync(customer);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _customerAddressRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerAddress>()))
            .ReturnsAsync((CustomerAddress a) => a);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.CreateAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Failure);

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNoCustomerProfile_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var request = new CreateCustomerAddressDTO
        {
            Address = "123 Main St",
            City = "Cairo",
            Country = "Egypt",
            PostalCode = "12345",
            AddressType = CustomerAddressType.Shipping
        };

        _customerRepositoryMock.Setup(r => r.GetCustomerByUserIdAsync(userId)).ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.CreateAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);

        _customerAddressRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CustomerAddress>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
