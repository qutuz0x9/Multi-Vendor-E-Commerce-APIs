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

public class GetByIdCustomerAddressTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICustomerAddressService _customerAddressService;

    public GetByIdCustomerAddressTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);

        _customerAddressService = new CustomerAddressService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAddressExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var customerId = Guid.NewGuid();
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

        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(address.Id);
        result.Value.Address.Should().Be(address.Address);
        result.Value.City.Should().Be(address.City);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAddressNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _customerAddressRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CustomerAddress?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }
}
