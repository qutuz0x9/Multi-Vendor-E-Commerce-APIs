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

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.BrandServiceTest;

public class GetAllBrandsTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<BrandService>> _loggerMock;
    private readonly IBrandService _brandService;

    public GetAllBrandsTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<BrandService>>();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _brandService = new BrandService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenBrandsExist_ShouldReturnMappedDTOs()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brands = new List<Brand>
        {
            new() { Id = 1, Name = "Nike",   NormalizedName = "NIKE",   Slug = "nike",   Status = BrandStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Adidas", NormalizedName = "ADIDAS", Slug = "adidas", Status = BrandStatus.Active, CreatedAt = DateTime.UtcNow }
        };
        _brandRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(brands);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(b => b.Name == "Nike");
        result.Value.Should().Contain(b => b.Name == "Adidas");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBrandsExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Brand>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
