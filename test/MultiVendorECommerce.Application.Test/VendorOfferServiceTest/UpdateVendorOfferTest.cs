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

public class UpdateVendorOfferTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly Mock<IVendorRepository> _vendorRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IVendorOfferService _vendorOfferService;

    public UpdateVendorOfferTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _vendorRepositoryMock = new Mock<IVendorRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Vendors).Returns(_vendorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Inventories).Returns(_inventoryRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedOffer()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var vendor = new Vendor { Id = vendorId, UserId = userId };
        var existingOffer = new VendorOffer
        {
            Id = 1,
            VendorId = vendorId,
            ProductId = 10,
            Price = 50m,
            Staus = VendorOfferStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Product = new Product { Id = 10, Name = "Test Product" }
        };
        var existingInventory = new Inventory
        {
            Id = 1,
            VendorOfferId = 1,
            Quantity = 20,
            ReservedQuantity = 5
        };
        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOffer);

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(1))
            .ReturnsAsync(existingInventory);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(true);

        _vendorOfferRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VendorOffer>()))
            .Returns(Task.CompletedTask);

        _inventoryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Inventory>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _unitOfWorkMock
            .Setup(u => u.CommitTransactionAsync())
            .ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.UpdateAsync(userId, 1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Price.Should().Be(request.Price);
        result.Value.Status.Should().Be(request.Status);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _vendorOfferRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VendorOffer>()), Times.Once);
        _inventoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Inventory>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenVendorNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync((Vendor?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.UpdateAsync(userId, 1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _vendorOfferRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOfferNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var vendor = new Vendor { Id = Guid.NewGuid(), UserId = userId };
        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((VendorOffer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.UpdateAsync(userId, 99, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenVendorDoesNotOwnOffer_ShouldReturnForbidden()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var vendor = new Vendor { Id = Guid.NewGuid(), UserId = userId };
        var existingOffer = new VendorOffer
        {
            Id = 1,
            VendorId = Guid.NewGuid(), // different vendor owns the offer
            ProductId = 10,
            Price = 50m,
            Staus = VendorOfferStatus.Active
        };
        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOffer);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.UpdateAsync(userId, 1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        _inventoryRepositoryMock.Verify(r => r.GetInventoryByVendorOfferAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenInventoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var vendor = new Vendor { Id = vendorId, UserId = userId };
        var existingOffer = new VendorOffer
        {
            Id = 1,
            VendorId = vendorId,
            ProductId = 10,
            Price = 50m,
            Staus = VendorOfferStatus.Active
        };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOffer);

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(1))
            .ReturnsAsync((Inventory?)null);

        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.UpdateAsync(userId, 1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_ShouldRollbackTransaction()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var vendor = new Vendor { Id = vendorId, UserId = userId };
        var existingOffer = new VendorOffer
        {
            Id = 1,
            VendorId = vendorId,
            ProductId = 10,
            Price = 50m,
            Staus = VendorOfferStatus.Active
        };
        var existingInventory = new Inventory { Id = 1, VendorOfferId = 1, Quantity = 20 };
        var request = new UpdateVendorOfferDTO { Price = 75m, Status = VendorOfferStatus.Active, Quantity = 30 };

        _vendorRepositoryMock
            .Setup(r => r.GetVendorByIdAsync(userId))
            .ReturnsAsync(vendor);

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOffer);

        _inventoryRepositoryMock
            .Setup(r => r.GetInventoryByVendorOfferAsync(1))
            .ReturnsAsync(existingInventory);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(true);

        _vendorOfferRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VendorOffer>()))
            .ThrowsAsync(new Exception("DB error"));

        _unitOfWorkMock
            .Setup(u => u.RollbackTransactionAsync())
            .ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var act = async () => await _vendorOfferService.UpdateAsync(userId, 1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        await act.Should().ThrowAsync<Exception>().WithMessage("DB error");

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }
}
