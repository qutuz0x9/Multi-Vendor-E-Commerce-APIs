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

namespace MultiVendorECommerce.Application.Test.CartItemServiceTest;

public class GetCartItemByIdTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICartItemService _cartItemService;

    public GetCartItemByIdTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);

        _cartItemService = new CartItemService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_ShouldReturnItem()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var cartSessionId = Guid.NewGuid();
        var item = new CartItem { Id = 1, VendorOfferId = 2, Quantity = 3, CartSessionId = cartSessionId };

        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(item);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Id.Should().Be(1);
        result.Value.VendorOfferId.Should().Be(2);
        result.Value.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemDoesNotExist_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _cartItemRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((CartItem?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);
    }
}
