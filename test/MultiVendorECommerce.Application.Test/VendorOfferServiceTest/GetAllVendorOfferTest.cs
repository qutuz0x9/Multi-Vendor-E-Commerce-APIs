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

namespace MultiVendorECommerce.Application.Test.VendorOfferServiceTest;

public class GetAllVendorOfferTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IVendorOfferService _vendorOfferService;

    public GetAllVendorOfferTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_WhenOffersExist_ShouldReturnMappedDTOs()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var offers = new List<VendorOffer>
        {
            new() { Id = 1, VendorId = vendorId, ProductId = 10, Price = 49.99m, Staus = VendorOfferStatus.Active, CreatedAt = DateTime.UtcNow, Product = new Product { Id = 10, Name = "Product A" } },
            new() { Id = 2, VendorId = vendorId, ProductId = 11, Price = 99.99m, Staus = VendorOfferStatus.Active, CreatedAt = DateTime.UtcNow, Product = new Product { Id = 11, Name = "Product B" } }
        };

        _vendorOfferRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(offers);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(o => o.Id == 1 && o.Price == 49.99m);
        result.Value.Should().Contain(o => o.Id == 2 && o.Price == 99.99m);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoOffersExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _vendorOfferRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<VendorOffer>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
