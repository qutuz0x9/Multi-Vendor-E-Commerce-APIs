using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.ProductServiceTest;

public class CreateProductTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;

    public CreateProductTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        // Default ProductCategory behavior
        _productCategoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ProductCategory>()))
            .ReturnsAsync((ProductCategory pc) => pc);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnCreatedProduct()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var categories = new List<Category>
        {
            new Category { Id = 10, Name = "Running" },
            new Category { Id = 20, Name = "Basketball" }
        };
        var request = new CreateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Running shoe",
            Feature = JsonDocument.Parse("""{"color":"red","size":"XL"}""").RootElement,
            CategoryIds = new List<int> { 10, 20 }
        };

        foreach (var category in categories)
        {
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);
        }

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(request.BrandId))
            .ReturnsAsync(brand);

        _productRepositoryMock
            .Setup(r => r.GetProductBySlugAsync("air-max-red-xl"))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 1)
            .ReturnsAsync((Product p) => p);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        result.Value.Slug.Should().Be("air-max-red-xl");
        result.Value.Description.Should().Be(request.Description);
        result.Value.Feature.Should().NotBeNull();
        result.Value.Categories.Should().HaveCount(2);
        result.Value.Status.Should().Be(ProductStatus.Drafted);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _brandRepositoryMock.Verify(r => r.GetByIdAsync(request.BrandId), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(20), Times.Once);
        _productRepositoryMock.Verify(r => r.GetProductBySlugAsync("air-max-red-xl"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2)); // Once for product, once for categories
    }

    [Fact]
    public async Task CreateAsync_WithFeature_ShouldPersistFeature()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var featureJson = JsonDocument.Parse("""{"color":"red","size":"XL"}""").RootElement;
        var request = new CreateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Running shoe",
            Feature = featureJson
        };

        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync((Product?)null);

        Product? capturedProduct = null;
        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => { p.Id = 1; capturedProduct = p; })
            .ReturnsAsync((Product p) => p);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Feature.Should().NotBeNull();
        capturedProduct.Feature!.RootElement.GetProperty("color").GetString().Should().Be("red");
        capturedProduct.Feature.RootElement.GetProperty("size").GetString().Should().Be("XL");
    }

    [Fact]
    public async Task CreateAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateProductDTO { BrandId = 99, Name = "Air Max", Description = "Running shoe" };

        _brandRepositoryMock
            .Setup(r => r.GetByIdAsync(request.BrandId))
            .ReturnsAsync((Brand?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnConflict()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var request = new CreateProductDTO { BrandId = 1, Name = "Air Max", Description = "Running shoe" };

        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock
            .Setup(r => r.GetProductBySlugAsync("air-max"))
            .ReturnsAsync(new Product { Id = 5, Slug = "air-max" });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(409);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var request = new CreateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Running shoe",
            CategoryIds = [99]
        };

        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync((Product?)null);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
    [Fact]
    public async Task CreateAsync_WhenOneCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var categories = new List<Category>
        {
            new Category { Id = 99, Name = "Running" },
            new Category { Id = 100, Name = "Basketball" }
        };
        var request = new CreateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Running shoe",
            CategoryIds = new List<int> { 99, 101 }
        };

        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync((Product?)null);
        foreach (var category in categories)
        {
            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);
        }

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _brandRepositoryMock.Verify(r => r.GetByIdAsync(request.BrandId), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(99), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(101), Times.Once);
    }
}
