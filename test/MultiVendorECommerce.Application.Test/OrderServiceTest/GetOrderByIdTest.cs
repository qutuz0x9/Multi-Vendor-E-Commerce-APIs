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

namespace MultiVendorECommerce.Application.Test.OrderServiceTest;

public class GetOrderByIdTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public GetOrderByIdTest()
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
    public async Task GetOrderByIdAsync_WhenOrderNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(99))
            .ReturnsAsync((Order?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetOrderByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderExists_ShouldReturnOrderWithItemsAndShippingAddress()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = 1,
            CustomerId = customerId,
            TotalAmount = 150m,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new() { Id = 10, OrderId = 1, VendorOfferId = 5, ProductName = "Test Product", Quantity = 3, UnitPrice = 50m, Price = 150m, CreatedAt = DateTime.UtcNow }
            },
            ShippingAddress = new OrderShippingAddress
            {
                Id = 7,
                OrderId = 1,
                ShippingAddress = "456 Elm St",
                ShippingCity = "Alexandria",
                ShippingCountry = "Egypt",
                ShippingPhoneNumber = "01098765432",
                CreatedAt = DateTime.UtcNow
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetOrderWithItemsAsync(1))
            .ReturnsAsync(order);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.GetOrderByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        var dto = result.Value;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(1);
        dto.TotalAmount.Should().Be(150m);
        dto.Status.Should().Be(OrderStatus.Confirmed);

        dto.OrderItems.Should().HaveCount(1);
        dto.OrderItems.First().ProductName.Should().Be("Test Product");
        dto.OrderItems.First().Quantity.Should().Be(3);

        dto.ShippingAddress.Should().NotBeNull();
        dto.ShippingAddress.ShippingCity.Should().Be("Alexandria");
        dto.ShippingAddress.ShippingPhoneNumber.Should().Be("01098765432");
    }
}
