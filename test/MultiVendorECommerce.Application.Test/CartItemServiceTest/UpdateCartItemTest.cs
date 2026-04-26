using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.CartItemServiceTest;

public class UpdateCartItemTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CartItemService>> _loggerMock;
    private readonly ICartItemService _cartItemService;

    public UpdateCartItemTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<CartItemService>>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);

        _cartItemService = new CartItemService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var request = new UpdateCartItemDTO { Quantity = 3 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartItemRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCartItemNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new UpdateCartItemDTO { Quantity = 3 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((CartItem?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.UpdateAsync(99, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _cartSessionRepositoryMock.Verify(r => r.GetCartByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCartItemBelongsToAnotherCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var otherCartSessionId = Guid.NewGuid();
        var request = new UpdateCartItemDTO { Quantity = 3 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new CartItem { Id = 1, CartSessionId = otherCartSessionId, VendorOfferId = 1, Quantity = 2 });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateAsync_WhenInsufficientStock_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new UpdateCartItemDTO { Quantity = 10 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new CartItem { Id = 1, CartSessionId = cartSessionId, VendorOfferId = 1, Quantity = 2 });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(1))
            .ReturnsAsync(new Inventory { VendorOfferId = 1, Quantity = 5, ReservedQuantity = 2 }); // 3 available

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_ShouldReturnUpdatedItem()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new UpdateCartItemDTO { Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new CartItem { Id = 1, CartSessionId = cartSessionId, VendorOfferId = 1, Quantity = 1 });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(1))
            .ReturnsAsync(new Inventory { VendorOfferId = 1, Quantity = 10, ReservedQuantity = 0 });

        _cartItemRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.UpdateAsync(1, userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Quantity.Should().Be(2);
    }
}
