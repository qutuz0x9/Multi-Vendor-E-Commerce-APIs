using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.VendorOfferServiceTest;

public class GetByIdVendorOfferTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<VendorOfferService>> _loggerMock;
    private readonly IVendorOfferService _vendorOfferService;

    public GetByIdVendorOfferTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<VendorOfferService>>();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOfferExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var offer = new VendorOffer
        {
            Id = 1,
            VendorId = Guid.NewGuid(),
            ProductId = 10,
            Price = 99.99m,
            Staus = VendorOfferStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Product = new Product { Id = 10, Name = "Test Product" }
        };

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(offer);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(offer.Id);
        result.Value.VendorId.Should().Be(offer.VendorId);
        result.Value.ProductId.Should().Be(offer.ProductId);
        result.Value.Price.Should().Be(offer.Price);
        result.Value.Status.Should().Be(offer.Staus);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOfferNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((VendorOffer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }
}
