using AutoMapper;
using FluentAssertions;
using Moq;
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

public class DeleteVendorOfferTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorOfferRepository> _vendorOfferRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<VendorOfferService>> _loggerMock;
    private readonly IVendorOfferService _vendorOfferService;

    public DeleteVendorOfferTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorOfferRepositoryMock = new Mock<IVendorOfferRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<VendorOfferService>>();

        _unitOfWorkMock.Setup(u => u.VendorOffers).Returns(_vendorOfferRepositoryMock.Object);

        _vendorOfferService = new VendorOfferService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenOfferExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var offer = new VendorOffer
        {
            Id = 1,
            VendorId = Guid.NewGuid(),
            ProductId = 10,
            Price = 99.99m,
            Staus = VendorOfferStatus.Active
        };

        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(offer);

        _vendorOfferRepositoryMock
            .Setup(r => r.DeleteAsync(offer))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.DeleteAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);

        _vendorOfferRepositoryMock.Verify(r => r.DeleteAsync(offer), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOfferNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _vendorOfferRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((VendorOffer?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _vendorOfferService.DeleteAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _vendorOfferRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<VendorOffer>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
