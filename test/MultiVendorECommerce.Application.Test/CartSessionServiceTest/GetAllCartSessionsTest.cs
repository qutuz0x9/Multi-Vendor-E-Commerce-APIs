using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.CartSession;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Models;

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.CartSessionServiceTest;

public class GetAllCartSessionsTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CartSessionService>> _loggerMock;
    private readonly ICartSessionService _cartSessionService;

    public GetAllCartSessionsTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<CartSessionService>>();

        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);

        _cartSessionService = new CartSessionService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenSessionsExist_ShouldReturnAllSessions()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var sessions = new List<CartSession>
        {
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };

        _cartSessionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(sessions);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoSessionsExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _cartSessionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<CartSession>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _cartSessionService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEmpty();
    }
}
