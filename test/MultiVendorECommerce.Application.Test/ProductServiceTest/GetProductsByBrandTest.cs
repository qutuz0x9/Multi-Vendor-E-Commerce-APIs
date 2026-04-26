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

public class GetProductsByBrandTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductService>> _loggerMock;
    private readonly IProductService _productService;

    public GetProductsByBrandTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProductsByBrandAsync_WhenBrandExists_ShouldReturnProducts()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Air Force", Slug = "air-force", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow }
        };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(brand);

        _productRepositoryMock
            .Setup(r => r.GetProductsByBrandAsync(1))
            .ReturnsAsync(products);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByBrandAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.All(p => p.BrandName == "Nike").Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsByBrandAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Brand?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByBrandAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.GetProductsByBrandAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByBrandAsync_WhenBrandHasNoProducts_ShouldReturnEmptyCollection()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Brand { Id = 1, Name = "Nike" });

        _productRepositoryMock
            .Setup(r => r.GetProductsByBrandAsync(1))
            .ReturnsAsync(new List<Product>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByBrandAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Should().BeEmpty();
    }
}
