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

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.BrandServiceTest;

public class GetByIdBrandTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<BrandService>> _loggerMock;
    private readonly IBrandService _brandService;

    public GetByIdBrandTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<BrandService>>();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _brandService = new BrandService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBrandExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand
        {
            Id = 1,
            Name = "Nike",
            NormalizedName = "NIKE",
            Slug = "nike",
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(brand);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(brand.Id);
        result.Value.Name.Should().Be(brand.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Brand?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBrandIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedBrand = new Brand
        {
            Id = 2,
            Name = "Archived Brand",
            NormalizedName = "ARCHIVED BRAND",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedBrand);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.GetByIdAsync(2);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }
}
