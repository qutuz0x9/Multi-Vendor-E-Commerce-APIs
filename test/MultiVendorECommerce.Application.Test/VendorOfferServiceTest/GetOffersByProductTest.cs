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

namespace MultiVendorECommerce.Application.Test.VendorOfferServiceTest;

public class GetOffersByProductTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IVendorOfferService _vendorOfferService;

    public GetOffersByProductTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetOffersByProductAsync_WhenProductExists_ShouldReturnOffers()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var product = new Product { Id = 10, Name = "Test Product" };
        var offers = new List<VendorOffer>
        {
            new() { Id = 1, VendorId = Guid.NewGuid(), ProductId = 10, Price = 50m, Staus = VendorOfferStatus.Active, Product = product },
            new() { Id = 2, VendorId = Guid.NewGuid(), ProductId = 10, Price = 45m, Staus = VendorOfferStatus.Active, Product = product }
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(product);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetOffersByProductAsync(10))
            .ReturnsAsync(offers);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetOffersByProductAsync(10);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().OnlyContain(o => o.ProductId == 10);
    }

    [Fact]
    public async Task GetOffersByProductAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Product?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetOffersByProductAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _vendorOfferRepositoryMock.Verify(r => r.GetOffersByProductAsync(It.IsAny<int>()), Times.Never);
    }
}
