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

public class DeleteCartSessionTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICartSessionService _cartSessionService;

    public DeleteCartSessionTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);

        _cartSessionService = new CartSessionService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsNotCustomer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetCustomerByUserIdAsync(userId))
            .ReturnsAsync((Customer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.DeleteAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.StatusCode.Should().Be(403);

        _cartSessionRepositoryMock.Verify(r => r.GetCartByCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCartSessionNotFound_ShouldReturnNotFound()
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
        var result = await _cartSessionService.DeleteAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.StatusCode.Should().Be(404);

        _cartSessionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CartSession>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenRequestIsValid_ShouldReturnNoContent()
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

        _cartSessionRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<CartSession>()))
            .Returns(Task.CompletedTask);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.DeleteAsync(userId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(204);

        _cartSessionRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<CartSession>()), Times.Once);
    }
}
