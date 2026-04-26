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

namespace MultiVendorECommerce.Application.Test.CartSessionServiceTest;

public class GetCartSessionByIdTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICartSessionService _cartSessionService;

    public GetCartSessionByIdTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);

        _cartSessionService = new CartSessionService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionExists_ShouldReturnSessionWithItems()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var sessionId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var session = new CartSession
        {
            Id = sessionId,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            CartItems = new List<CartItem>
            {
                new() { Id = 1, VendorOfferId = 1, Quantity = 2, CartSessionId = sessionId }
            }
        };

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartWithItemsAsync(sessionId))
            .ReturnsAsync(session);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.GetByIdAsync(sessionId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Id.Should().Be(sessionId);
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.CartItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionDoesNotExist_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var sessionId = Guid.NewGuid();

        _cartSessionRepositoryMock
            .Setup(r => r.GetCartWithItemsAsync(sessionId))
            .ReturnsAsync((CartSession?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.GetByIdAsync(sessionId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);
    }
}
