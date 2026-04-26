using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.OrderServiceTest;

public class UpdateOrderStatusTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public UpdateOrderStatusTest()
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
    public async Task UpdateOrderStatusAsync_WhenOrderNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(99))
            .ReturnsAsync((Order?)null);

        var request = new UpdateOrderStatusDTO { Status = OrderStatus.Confirmed };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.UpdateOrderStatusAsync(99, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task UpdateOrderStatusAsync_WhenOrderExists_ShouldUpdateStatusAndReturnDTO(OrderStatus newStatus)
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = 1,
            CustomerId = customerId,
            TotalAmount = 300m,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new() { Id = 10, OrderId = 1, VendorOfferId = 5, ProductName = "Widget", Quantity = 2, UnitPrice = 150m, Price = 300m, CreatedAt = DateTime.UtcNow }
            },
            ShippingAddress = new OrderShippingAddress
            {
                Id = 3,
                OrderId = 1,
                ShippingAddress = "1 Main Rd",
                ShippingCity = "Cairo",
                ShippingCountry = "Egypt",
                ShippingPhoneNumber = "01000000000",
                CreatedAt = DateTime.UtcNow
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateOrderStatusDTO { Status = newStatus };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.UpdateOrderStatusAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        var dto = result.Value;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(1);
        dto.Status.Should().Be(newStatus);
        dto.TotalAmount.Should().Be(300m);
        dto.OrderItems.Should().HaveCount(1);
        dto.ShippingAddress.Should().NotBeNull();
        dto.ShippingAddress.ShippingCity.Should().Be("Cairo");

        // Verify order was updated in repository
        order.Status.Should().Be(newStatus);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
