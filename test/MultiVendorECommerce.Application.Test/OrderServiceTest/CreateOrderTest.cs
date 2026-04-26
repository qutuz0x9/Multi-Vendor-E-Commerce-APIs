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

public class CreateOrderTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICustomerAddressRepository> _customerAddressRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly Mock<IOrderShippingAddressRepository> _orderShippingAddressRepositoryMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly IMapper _mapper;
    private readonly IOrderService _orderService;

    public CreateOrderTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _customerAddressRepositoryMock = new Mock<ICustomerAddressRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _orderShippingAddressRepositoryMock = new Mock<IOrderShippingAddressRepository>();

        var userStoreMock = Mock.Of<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock, // 1. IUserStore<User>
            null!,          // 2. IOptions<IdentityOptions>
            null!,          // 3. IPasswordHasher<User>
            null!,          // 4. IEnumerable<IUserValidator<User>>
            null!,          // 5. IEnumerable<IPasswordValidator<User>>
            null!,          // 6. ILookupNormalizer
            null!,          // 7. IdentityErrorDescriber
            null!,          // 8. IServiceProvider
            null!           // 9. ILogger<UserManager<User>>
        );

        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CustomerAddresses).Returns(_customerAddressRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.OrderItems).Returns(_orderItemRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.OrderShippingAddresses).Returns(_orderShippingAddressRepositoryMock.Object);

        _orderService = new OrderService(_unitOfWorkMock.Object, _mapper, _userManagerMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure: Phase 1 – Customer validation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartSessionRepositoryMock.Verify(r => r.GetCartByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCartSessionNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync((CartSession?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _cartItemRepositoryMock.Verify(r => r.GetItemsByCartAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCartIsEmpty_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _vendorOfferRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure: Phase 1 – Per-item validation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_WhenVendorOfferNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 99, Quantity = 2, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((VendorOffer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _inventoryRepositoryMock.Verify(r => r.GetInventoryByVendorOfferAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenOfferIsNotActive_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 5, Quantity = 2, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = 100m, Staus = VendorOfferStatus.Inactive });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _inventoryRepositoryMock.Verify(r => r.GetInventoryByVendorOfferAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenInventoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 5, Quantity = 2, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = 100m, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync((Inventory?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenInsufficientStock_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 5, Quantity = 10, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = 100m, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync(new Inventory { Id = 1, VendorOfferId = 5, Quantity = 8, ReservedQuantity = 5 }); // 3 available, 10 requested

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 5, Quantity = 2, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = 100m, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync(new Inventory { Id = 1, VendorOfferId = 5, Quantity = 20, ReservedQuantity = 0 });

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync((Product?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _customerAddressRepositoryMock.Verify(
            r => r.GetAddressesByTypeAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Failure: Phase 1 – Shipping address validation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_WhenNoShippingAddress_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem> { new() { Id = 1, VendorOfferId = 5, Quantity = 2, CartSessionId = cartSessionId } });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = 100m, Staus = VendorOfferStatus.Active });

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync(new Inventory { Id = 1, VendorOfferId = 5, Quantity = 20, ReservedQuantity = 0 });

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Product { Id = 10, Name = "Test Product" });

        _customerAddressRepositoryMock
            .Setup(r => r.GetAddressesByTypeAsync(customerId, (int)CustomerAddressType.Shipping))
            .ReturnsAsync(new List<CustomerAddress>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Success: happy path
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ShouldCreateOrderAndReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var cartSessionId = Guid.NewGuid();
        const decimal offerPrice = 50m;
        const int requestedQty = 3;
        const decimal expectedTotal = offerPrice * requestedQty; // 150m

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync(new Customer { Id = customerId, UserId = userId });

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartByCustomerAsync(customerId))
            .ReturnsAsync(new CartSession { Id = cartSessionId, CustomerId = customerId });

        _cartItemRepositoryMock
            .Setup(r => r.GetItemsByCartAsync(cartSessionId))
            .ReturnsAsync(new List<CartItem>
            {
                new() { Id = 1, VendorOfferId = 5, Quantity = requestedQty, CartSessionId = cartSessionId }
            });

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new VendorOffer { Id = 5, ProductId = 10, Price = offerPrice, Staus = VendorOfferStatus.Active });

        var inventory = new Inventory { Id = 1, VendorOfferId = 5, Quantity = 20, ReservedQuantity = 0 };
        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(5))
            .ReturnsAsync(inventory);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Product { Id = 10, Name = "Test Product" });

        _customerAddressRepositoryMock
            .Setup(r => r.GetAddressesByTypeAsync(customerId, (int)CustomerAddressType.Shipping))
            .ReturnsAsync(new List<CustomerAddress>
            {
                new()
                {
                    Id = 1,
                    CustomerId = customerId,
                    Address = "123 Main St",
                    City = "Cairo",
                    Country = "Egypt",
                    PostalCode = "12345",
                    AddressType = CustomerAddressType.Shipping
                }
            });

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new User { Id = userId, PhoneNumber = "01012345678" });

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _orderItemRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OrderItem>()))
            .ReturnsAsync((OrderItem oi) => oi);
        _inventoryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Inventory>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _orderShippingAddressRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OrderShippingAddress>()))
            .ReturnsAsync((OrderShippingAddress a) => a);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _orderService.CreateOrderAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);

        var orderDto = result.Value;
        orderDto.Should().NotBeNull();
        orderDto!.Status.Should().Be(OrderStatus.Pending);
        orderDto.TotalAmount.Should().Be(expectedTotal);
        orderDto.OrderItems.Should().HaveCount(1);

        var itemDto = orderDto.OrderItems.First();
        itemDto.ProductName.Should().Be("Test Product");
        itemDto.Quantity.Should().Be(requestedQty);
        itemDto.UnitPrice.Should().Be(offerPrice);
        itemDto.Price.Should().Be(expectedTotal);

        orderDto.ShippingAddress.Should().NotBeNull();
        orderDto.ShippingAddress.ShippingAddress.Should().Be("123 Main St");
        orderDto.ShippingAddress.ShippingCity.Should().Be("Cairo");
        orderDto.ShippingAddress.ShippingCountry.Should().Be("Egypt");
        orderDto.ShippingAddress.ShippingPhoneNumber.Should().Be("01012345678");

        // Verify inventory was reserved
        inventory.ReservedQuantity.Should().Be(requestedQty);
        _inventoryRepositoryMock.Verify(r => r.UpdateAsync(inventory), Times.Once);

        // Verify full transaction lifecycle
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrderItem>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _orderShippingAddressRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrderShippingAddress>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }
}
