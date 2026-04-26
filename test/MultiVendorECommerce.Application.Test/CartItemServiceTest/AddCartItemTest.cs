using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.CartItemServiceTest;

public class AddCartItemTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CartItemService>> _loggerMock;
    private readonly ICartItemService _cartItemService;

    public AddCartItemTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<CartItemService>>();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);

        _cartItemService = new CartItemService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task AddItemAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 1, Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartSessionRepositoryMock.Verify(r => r.GetCartByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenCartSessionNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 1, Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync((CartSession?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _vendorOfferRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenVendorOfferNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 99, Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(request.VendorOfferId))
            .ReturnsAsync((VendorOffer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _inventoryRepositoryMock.Verify(r => r.GetInventoryByVendorOfferAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenOfferIsNotActive_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 1, Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(request.VendorOfferId))
            .ReturnsAsync(new VendorOffer { Id = 1, Staus = VendorOfferStatus.Inactive });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _inventoryRepositoryMock.Verify(r => r.GetInventoryByVendorOfferAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenInsufficientStock_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 1, Quantity = 10 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(request.VendorOfferId))
            .ReturnsAsync(new VendorOffer { Id = 1, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(request.VendorOfferId))
            .ReturnsAsync(new Inventory { VendorOfferId = 1, Quantity = 5, ReservedQuantity = 2 }); // 3 available

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _cartItemRepositoryMock.Verify(r => r.GetCartItemByVendorOfferAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenItemAlreadyInCart_ShouldUpdateQuantity()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 1, Quantity = 3 };

        var existingCartItem = new CartItem
        {
            Id = 42,
            CartSessionId = cartSessionId,
            VendorOfferId = 1,
            Quantity = 1
        };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(request.VendorOfferId))
            .ReturnsAsync(new VendorOffer { Id = 1, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(request.VendorOfferId))
            .ReturnsAsync(new Inventory { VendorOfferId = 1, Quantity = 20, ReservedQuantity = 0 });

        _cartItemRepositoryMock
            .Setup(r => r.GetCartItemByVendorOfferAsync(cartSessionId, request.VendorOfferId))
            .ReturnsAsync(existingCartItem);

        _cartItemRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<CartItem>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(existingCartItem.Id);
        result.Value.Quantity.Should().Be(request.Quantity);
        result.StatusCode.Should().Be(200);

        _cartItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CartItem>()), Times.Once);
        _cartItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_WithValidData_ShouldReturnCreatedCartItem()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        var request = new AddCartItemDTO { VendorOfferId = 5, Quantity = 2 };

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(request.VendorOfferId))
            .ReturnsAsync(new VendorOffer { Id = 5, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(request.VendorOfferId))
            .ReturnsAsync(new Inventory { VendorOfferId = 5, Quantity = 10, ReservedQuantity = 1 }); // 9 available

        _cartItemRepositoryMock
            .Setup(r => r.GetCartItemByVendorOfferAsync(cartSessionId, request.VendorOfferId))
            .ReturnsAsync((CartItem?)null);

        _cartItemRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CartItem>()))
            .ReturnsAsync((CartItem item) => { item.Id = 1; return item; });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.AddItemAsync(userId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.VendorOfferId.Should().Be(request.VendorOfferId);
        result.Value.Quantity.Should().Be(request.Quantity);
        result.StatusCode.Should().Be(200);

        _cartItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Once);
        _cartItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CartItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
