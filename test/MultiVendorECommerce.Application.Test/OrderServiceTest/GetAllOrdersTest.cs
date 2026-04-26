using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;

namespace MultiVendorECommerce.Application.Test.OrderServiceTest;

public class GetAllOrdersTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public GetAllOrdersTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();

        var userStoreMock = Mock.Of<IUserStore<User>>();
        var userManagerMock = new Mock<UserManager<User>>(
            userStoreMock, null!, null!, null!, null!, null!, null!, null!, null!);

        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);

        _orderService = new OrderService(_unitOfWorkMock.Object, _mapper, userManagerMock.Object);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WhenOrdersExist_ShouldReturnMappedDTOs()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { Id = 1, CustomerId = customerId, TotalAmount = 100m, Status = OrderStatus.Pending,   CreatedAt = DateTime.UtcNow },
            new() { Id = 2, CustomerId = customerId, TotalAmount = 250m, Status = OrderStatus.Confirmed, CreatedAt = DateTime.UtcNow }
        };
        _orderRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetAllOrdersAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Select(o => o.Id).Should().BeEquivalentTo([1, 2]);
        result.Value.Select(o => o.TotalAmount).Should().BeEquivalentTo([100m, 250m]);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WhenNoOrdersExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _orderRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetAllOrdersAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
