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

public class GetOffersByVendorTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IVendorRepository> _vendorRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<VendorOfferService>> _loggerMock;
    private readonly IVendorOfferService _vendorOfferService;

    public GetOffersByVendorTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _vendorRepositoryMock = new Mock<IVendorRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<VendorOfferService>>();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Vendors).Returns(_vendorRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOffersByVendorAsync_WhenVendorExists_ShouldReturnOffers()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var vendor = new Vendor { Id = vendorId, BusinessName = "Test Vendor" };
        var offers = new List<VendorOffer>
        {
            new() { Id = 1, VendorId = vendorId, ProductId = 10, Price = 50m, Staus = VendorOfferStatus.Active, Product = new Product { Id = 10, Name = "Product A" } },
            new() { Id = 2, VendorId = vendorId, ProductId = 11, Price = 80m, Staus = VendorOfferStatus.Active, Product = new Product { Id = 11, Name = "Product B" } }
        };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetOffersByVendorAsync(vendorId))
            .ReturnsAsync(offers);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetOffersByVendorAsync(vendorId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOffersByVendorAsync_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync((Vendor?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetOffersByVendorAsync(vendorId);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _vendorOfferRepositoryMock.Verify(r => r.GetOffersByVendorAsync(It.IsAny<Guid>()), Times.Never);
    }
}
