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

namespace MultiVendorECommerce.Application.Test.BrandServiceTest;

public class DeleteBrandTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IBrandService _brandService;

    public DeleteBrandTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _brandService = new BrandService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand
        {
            Id = 1,
            Name = "Nike",
            NormalizedName = "NIKE",
            IsDeleted = false
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(brand);

        _brandRepositoryMock
            .Setup(r => r.DeleteAsync(brand))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.DeleteAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);

        _brandRepositoryMock.Verify(r => r.DeleteAsync(brand), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Brand?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.DeleteAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _brandRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Brand>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBrandIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedBrand = new Brand
        {
            Id = 2,
            Name = "Archived Brand",
            NormalizedName = "ARCHIVED BRAND",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-3)
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedBrand);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.DeleteAsync(2);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _brandRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Brand>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
