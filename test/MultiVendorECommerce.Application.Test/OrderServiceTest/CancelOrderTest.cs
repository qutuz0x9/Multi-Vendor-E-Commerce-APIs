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

public class CancelOrderTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public CancelOrderTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();

        var userStoreMock = Mock.Of<IUserStore<User>>();
        var userManagerMock = new Mock<UserManager<User>>(
            userStoreMock, null!, null!, null!, null!, null!, null!, null!, null!);

        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);

        _orderService = new OrderService(_unitOfWorkMock.Object, _mapper, userManagerMock.Object, new Mock<IAppLogger<OrderService>>().Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure paths
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrderAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CancelOrderAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _orderRepositoryMock.Verify(r => r.GetOrderWithItemsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(99))
            .ReturnsAsync((Order?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CancelOrderAsync(99, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderBelongsToDifferentCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(new Order { Id = 1, CustomerId = otherCustomerId, Status = OrderStatus.Pending });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CancelOrderAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelOrderAsync_WhenOrderStatusIsNotCancellable_ShouldReturnValidationFailure(OrderStatus status)
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(new Order { Id = 1, CustomerId = customerId, Status = status, OrderItems = new List<OrderItem>() });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CancelOrderAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Success paths
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    public async Task CancelOrderAsync_WhenOrderIsCancellable_ShouldCancelAndReleaseInventory(OrderStatus status)
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        var inventory = new Inventory { Id = 1, VendorOfferId = 5, Quantity = 20, ReservedQuantity = 3 };
        var order = new Order
        {
            Id = 1,
            CustomerId = customerId,
            Status = status,
            OrderItems = new List<OrderItem>
            {
                new() { Id = 10, OrderId = 1, VendorOfferId = 5, Quantity = 3 }
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(order);

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync(inventory);

        _inventoryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Inventory>()))
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CancelOrderAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        // Verify order status was set to Cancelled
        order.Status.Should().Be(OrderStatus.Cancelled);

        // Verify inventory was released
        inventory.ReservedQuantity.Should().Be(0);
        _inventoryRepositoryMock.Verify(r => r.UpdateAsync(inventory), Times.Once);

        _orderRepositoryMock.Verify(r => r.UpdateAsync(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
