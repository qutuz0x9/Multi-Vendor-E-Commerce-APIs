using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.CartItemServiceTest;

public class RemoveCartItemTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICartItemService _cartItemService;

    public RemoveCartItemTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);

        _cartItemService = new CartItemService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.RemoveItemAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartItemRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenCartItemNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((CartItem?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.RemoveItemAsync(99, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _cartSessionRepositoryMock.Verify(r => r.GetCartByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenCartItemBelongsToAnotherCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var otherCartSessionId = Guid.NewGuid();

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
        var result = await _cartItemService.RemoveItemAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartItemRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_WhenRequestIsValid_ShouldReturnNoContent()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new CartItem { Id = 1, CartSessionId = cartSessionId, VendorOfferId = 1, Quantity = 2 });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.RemoveItemAsync(1, userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(204);

        _cartItemRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CartItem>()), Times.Once);
    }
}
