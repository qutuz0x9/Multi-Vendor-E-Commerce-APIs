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

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.ProductServiceTest;

public class UpdateProductTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductService>> _loggerMock;
    private readonly IProductService _productService;

    public UpdateProductTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        // Default ProductCategory behavior: no existing categories
        _productCategoryRepositoryMock
            .Setup(r => r.GetCategoriesByProductAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ProductCategory>());

        _productCategoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ProductCategory>()))
            .ReturnsAsync((ProductCategory pc) => pc);

        _productCategoryRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<ProductCategory>()))
            .Returns(Task.CompletedTask);

        // Default transaction behavior
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).ReturnsAsync(true);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedProduct()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var existing = new Product
        {
            Id = 1,
            BrandId = 1,
            Name = "Old Name",
            Description = "Old description",
            Slug = "old-name",
            Status = ProductStatus.Drafted,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
        };

        var request = new UpdateProductDTO
        {
            BrandId = 1,
            Name = "Air Max Pro",
            Description = "Updated description",
            Status = ProductStatus.Active
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max-pro")).ReturnsAsync((Product?)null);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        result.Value.Slug.Should().Be("air-max-pro");
        result.Value.Status.Should().Be(request.Status);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithCategories_ShouldReplaceProductCategories()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var newCategory = new Category { Id = 20, Name = "Lifestyle" };
        var oldProductCategory = new ProductCategory { Id = 5, ProductId = 1, CategoryId = 10 };

        var existing = new Product
        {
            Id = 1,
            BrandId = 1,
            Name = "Air Max",
            Description = "Old",
            Slug = "air-max",
            IsDeleted = false,
        };

        var request = new UpdateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Updated",
            Status = ProductStatus.Active,
            CategoryIds = [20]
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync(existing);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(newCategory);

        _productCategoryRepositoryMock
            .Setup(r => r.GetCategoriesByProductAsync(1))
            .ReturnsAsync(new List<ProductCategory> { oldProductCategory });

        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        result.Value!.Categories.Should().HaveCount(1);
        _productCategoryRepositoryMock.Verify(r => r.DeleteAsync(oldProductCategory), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithFeature_ShouldPersistUpdatedFeature()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var existing = new Product
        {
            Id = 1,
            BrandId = 1,
            Name = "Air Max",
            Description = "Old",
            Slug = "air-max",
            IsDeleted = false,
            Brand = brand
        };

        var request = new UpdateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Updated",
            Feature = JsonDocument.Parse("""{"color":"blue"}""").RootElement,
            Status = ProductStatus.Active
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync(existing);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        existing.Feature.Should().NotBeNull();
        existing.Feature!.RootElement.GetProperty("color").GetString().Should().Be("blue");
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);
        var request = new UpdateProductDTO { BrandId = 1, Name = "Air Max", Description = "Desc", Status = ProductStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(99, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductIsSoftDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deleted = new Product { Id = 1, Name = "Air Max", Slug = "air-max", IsDeleted = true };
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(deleted);
        var request = new UpdateProductDTO { BrandId = 1, Name = "Air Max", Description = "Desc", Status = ProductStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenBrandNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var existing = new Product { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, IsDeleted = false };
        var request = new UpdateProductDTO { BrandId = 99, Name = "Air Max", Description = "Desc", Status = ProductStatus.Active };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync((Brand?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ShouldReturnConflict()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var existing = new Product { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, IsDeleted = false };
        var conflicting = new Product { Id = 2, Slug = "air-force" };
        var request = new UpdateProductDTO { BrandId = 1, Name = "Air Force", Description = "Desc", Status = ProductStatus.Active };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-force")).ReturnsAsync(conflicting);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(409);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var existing = new Product { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, IsDeleted = false, Brand = brand };
        var request = new UpdateProductDTO
        {
            BrandId = 1,
            Name = "Air Max",
            Description = "Desc",
            Status = ProductStatus.Active,
            CategoryIds = [99]
        };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _brandRepositoryMock.Setup(r => r.GetByIdAsync(request.BrandId)).ReturnsAsync(brand);
        _productRepositoryMock.Setup(r => r.GetProductBySlugAsync("air-max")).ReturnsAsync(existing);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}
