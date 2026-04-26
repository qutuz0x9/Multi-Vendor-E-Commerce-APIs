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

public class GetAllCartItemsTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartItemRepository> _cartItemRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICartItemService _cartItemService;

    public GetAllCartItemsTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartItemRepositoryMock = new Mock<ICartItemRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartItemRepositoryMock.Object);

        _cartItemService = new CartItemService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_WhenItemsExist_ShouldReturnAllItems()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var items = new List<CartItem>
        {
            new() { Id = 1, VendorOfferId = 1, Quantity = 2, CartSessionId = Guid.NewGuid() },
            new() { Id = 2, VendorOfferId = 3, Quantity = 1, CartSessionId = Guid.NewGuid() }
        };

        _cartItemRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(items);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoItemsExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _cartItemRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CartItem>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartItemService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEmpty();
    }
}
