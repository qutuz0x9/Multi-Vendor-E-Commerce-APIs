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

public class GetProductsByCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductService>> _loggerMock;
    private readonly IProductService _productService;

    public GetProductsByCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryExists_ShouldReturnProducts()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var category = new Category { Id = 1, Name = "Running" };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Air Force", Slug = "air-force", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow }
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(r => r.GetProductsByCategoryAsync(1))
            .ReturnsAsync(products);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByCategoryAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByCategoryAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.GetProductsByCategoryAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryHasNoProducts_ShouldReturnEmptyCollection()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Category { Id = 1, Name = "Running" });

        _productRepositoryMock
            .Setup(r => r.GetProductsByCategoryAsync(1))
            .ReturnsAsync(new List<Product>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetProductsByCategoryAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value!.Should().BeEmpty();
    }
}
