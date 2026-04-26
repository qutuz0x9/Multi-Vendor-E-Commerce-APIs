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
using System.Linq.Expressions;

namespace MultiVendorECommerce.Application.Test.CustomerAddressServiceTest;

public class GetByTypeCustomerAddressTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICustomerAddressService _customerAddressService;

    public GetByTypeCustomerAddressTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);

        _customerAddressService = new CustomerAddressService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenMatchingAddressesExist_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var addressType = CustomerAddressType.Shipping;
        var addresses = new List<CustomerAddress>
        {
            new() { Id = 1, CustomerId = Guid.NewGuid(), Address = "123 Ship St", City = "Cairo", Country = "Egypt", PostalCode = "11111", AddressType = CustomerAddressType.Shipping, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, CustomerId = Guid.NewGuid(), Address = "456 Ship Rd", City = "Giza", Country = "Egypt", PostalCode = "22222", AddressType = CustomerAddressType.Shipping, CreatedAt = DateTime.UtcNow }
        };

        _customerAddressRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomerAddress, bool>>>()))
            .ReturnsAsync(addresses);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetByTypeAsync(addressType);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenNoMatchingAddresses_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _customerAddressRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomerAddress, bool>>>()))
            .ReturnsAsync(new List<CustomerAddress>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _customerAddressService.GetByTypeAsync(CustomerAddressType.Pickup);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Should().BeEmpty();
    }
}
