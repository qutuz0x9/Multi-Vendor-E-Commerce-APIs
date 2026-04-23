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

public class CreateVendorOfferTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IVendorRepository> _vendorRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IVendorOfferService _vendorOfferService;

    public CreateVendorOfferTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _vendorRepositoryMock = new Mock<IVendorRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Vendors).Returns(_vendorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnCreatedOffer()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var request = new CreateVendorOfferDTO { ProductId = 10, Price = 99.99m, Quantity = 50 };

        var vendor = new Vendor { Id = vendorId, BusinessName = "Test Vendor" };
        var product = new Product { Id = 10, Name = "Test Product" };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(request.ProductId))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(true);

        _vendorOfferRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<VendorOffer>()))
            .ReturnsAsync((VendorOffer vo) => { vo.Id = 1; vo.Product = product; return vo; });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _inventoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory inv) => inv);

        _unitOfWorkMock
            .Setup(u => u.CommitTransactionAsync())
            .ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.CreateAsync(vendorId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.VendorId.Should().Be(vendorId);
        result.Value.ProductId.Should().Be(request.ProductId);
        result.Value.Price.Should().Be(request.Price);
        result.Value.Status.Should().Be(VendorOfferStatus.Active);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _vendorOfferRepositoryMock.Verify(r => r.AddAsync(It.IsAny<VendorOffer>()), Times.Once);
        _inventoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Inventory>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var request = new CreateVendorOfferDTO { ProductId = 10, Price = 99.99m, Quantity = 50 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync((Vendor?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.CreateAsync(vendorId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        _vendorOfferRepositoryMock.Verify(r => r.AddAsync(It.IsAny<VendorOffer>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var request = new CreateVendorOfferDTO { ProductId = 99, Price = 99.99m, Quantity = 50 };

        var vendor = new Vendor { Id = vendorId, BusinessName = "Test Vendor" };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(request.ProductId))
            .ReturnsAsync((Product?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.CreateAsync(vendorId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        _vendorOfferRepositoryMock.Verify(r => r.AddAsync(It.IsAny<VendorOffer>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenExceptionThrown_ShouldRollbackTransaction()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var vendorId = Guid.NewGuid();
        var request = new CreateVendorOfferDTO { ProductId = 10, Price = 99.99m, Quantity = 50 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(vendorId))
            .ReturnsAsync(new Vendor { Id = vendorId });

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(request.ProductId))
            .ReturnsAsync(new Product { Id = 10, Name = "Test Product" });

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(true);

        _vendorOfferRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<VendorOffer>()))
            .ThrowsAsync(new Exception("DB error"));

        _unitOfWorkMock
            .Setup(u => u.RollbackTransactionAsync())
            .ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var act = async () => await _vendorOfferService.CreateAsync(vendorId, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        await act.Should().ThrowAsync<Exception>().WithMessage("DB error");

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }
}
