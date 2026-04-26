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
namespace MultiVendorECommerce.Application.Test.ProductServiceTest;

public class GetByIdProductTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductService>> _loggerMock;
    private readonly IProductService _productService;

    public GetByIdProductTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var product = new Product
        {
            Id = 1,
            BrandId = 1,
            Name = "Air Max",
            Description = "Running shoe",
            Slug = "air-max",
            Status = ProductStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Brand = brand
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be(product.Name);
        result.Value.Slug.Should().Be(product.Slug);
        result.Value.BrandName.Should().Be(brand.Name);
        result.Value.Status.Should().Be(product.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Product?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductIsSoftDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var product = new Product
        {
            Id = 1,
            Name = "Air Max",
            Slug = "air-max",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }
}
