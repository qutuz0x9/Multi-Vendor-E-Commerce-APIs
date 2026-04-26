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
using MultiVendorECommerce.Shared.Enums;
using MultiVendorECommerce.Shared.Logging;

namespace MultiVendorECommerce.Application.Test.OrderServiceTest;

public class GetMyOrdersTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public GetMyOrdersTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();

        var userStoreMock = Mock.Of<IUserStore<User>>();
        var userManagerMock = new Mock<UserManager<User>>(
            userStoreMock, null!, null!, null!, null!, null!, null!, null!, null!);

        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);

        _orderService = new OrderService(_unitOfWorkMock.Object, _mapper, userManagerMock.Object, new Mock<IAppLogger<OrderService>>().Object);
    }

    [Fact]
    public async Task GetMyOrdersAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetMyOrdersAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _orderRepositoryMock.Verify(r => r.GetOrdersByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetMyOrdersAsync_WhenCustomerHasOrders_ShouldReturnOrders()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByCustomerAsync(customerId))
            .ReturnsAsync(new List<Order>
            {
                new() { Id = 1, CustomerId = customerId, TotalAmount = 100m, Status = OrderStatus.Pending,   CreatedAt = DateTime.UtcNow },
                new() { Id = 2, CustomerId = customerId, TotalAmount = 200m, Status = OrderStatus.Delivered, CreatedAt = DateTime.UtcNow }
            });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetMyOrdersAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Select(o => o.Id).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task GetMyOrdersAsync_WhenCustomerHasNoOrders_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _orderRepositoryMock
            .Setup(r => r.GetOrdersByCustomerAsync(customerId))
            .ReturnsAsync([]);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetMyOrdersAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
