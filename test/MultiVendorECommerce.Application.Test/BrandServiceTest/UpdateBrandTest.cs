using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.BrandServiceTest;

public class UpdateBrandTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IBrandService _brandService;

    public UpdateBrandTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _brandService = new BrandService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedBrand()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var existing = new Brand
        {
            Id = 1,
            Name = "Old Brand",
            NormalizedName = "OLD BRAND",
            Slug = "old-brand",
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var request = new UpdateBrandDTO
        {
            Name = "New Brand",
            Status = BrandStatus.Active
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _brandRepositoryMock
            .Setup(r => r.GetBrandByNameAsync(request.Name))
            .ReturnsAsync((Brand?)null);

        _brandRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        

        _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithSameName_ShouldReturnUpdatedBrand()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var existing = new Brand
        {
            Id = 1,
            Name = "Nike",
            NormalizedName = "NIKE",
            Slug = "nike",
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var request = new UpdateBrandDTO
        {
            Name = "Nike",
            Status = BrandStatus.Inactive
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        // Simulates the existing brand being found (same brand, same normalized name → no conflict)
        _brandRepositoryMock
            .Setup(r => r.GetBrandByNameAsync(request.Name))
            .ReturnsAsync(existing);

        _brandRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Nike");
    }

    [Fact]
    public async Task UpdateAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Brand?)null);

        var request = new UpdateBrandDTO { Name = "Nike", Status = BrandStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.UpdateAsync(99, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBrandIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedBrand = new Brand
        {
            Id = 2,
            Name = "Archived",
            NormalizedName = "ARCHIVED",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedBrand);

        var request = new UpdateBrandDTO { Name = "Archived Updated", Status = BrandStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.UpdateAsync(2, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WithConflictingName_ShouldReturnConflict()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var existing = new Brand
        {
            Id = 1,
            Name = "Nike",
            NormalizedName = "NIKE",
            Slug = "nike",
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var conflictingBrand = new Brand
        {
            Id = 2,
            Name = "Adidas",
            NormalizedName = "ADIDAS"
        };

        var request = new UpdateBrandDTO { Name = "Adidas", Status = BrandStatus.Active };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        // A different brand already owns the name "Adidas"
        _brandRepositoryMock
            .Setup(r => r.GetBrandByNameAsync(request.Name))
            .ReturnsAsync(conflictingBrand);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(409);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _brandRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
