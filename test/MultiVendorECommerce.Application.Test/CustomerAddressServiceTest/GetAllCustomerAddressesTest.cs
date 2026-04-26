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

namespace MultiVendorECommerce.Application.Test.CustomerAddressServiceTest;

public class GetAllCustomerAddressesTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICustomerAddressService _customerAddressService;

    public GetAllCustomerAddressesTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);

        _customerAddressService = new CustomerAddressService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_WhenAddressesExist_ShouldReturnAll()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var addresses = new List<CustomerAddress>
        {
            new() { Id = 1, CustomerId = Guid.NewGuid(), Address = "123 Main St", City = "Cairo", Country = "Egypt", PostalCode = "12345", AddressType = CustomerAddressType.Shipping, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, CustomerId = Guid.NewGuid(), Address = "789 Billing Ave", City = "Giza", Country = "Egypt", PostalCode = "67890", AddressType = CustomerAddressType.Billing, CreatedAt = DateTime.UtcNow }
        };

        _customerAddressRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(addresses);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoAddressesExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _customerAddressRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CustomerAddress>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Should().BeEmpty();
    }
}
