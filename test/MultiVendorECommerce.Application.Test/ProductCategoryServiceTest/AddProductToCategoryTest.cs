using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.ProductCategoryServiceTest;

public class AddProductToCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductCategoryService>> _loggerMock;
    private readonly IProductCategoryService _productCategoryService;

    public AddProductToCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductCategoryService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        _productCategoryService = new ProductCategoryService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task AddProductToCategoryAsync_WithValidRequest_ShouldReturnCreated()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateProductCategoryDTO { ProductId = 1, CategoryId = 10 };

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>()))
            .ReturnsAsync(false);

        _productCategoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ProductCategory>()))
            .ReturnsAsync((ProductCategory pc) => pc);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.AddProductToCategoryAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.ProductId.Should().Be(request.ProductId);
        result.Value.CategoryId.Should().Be(request.CategoryId);

        _productRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddProductToCategoryAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateProductCategoryDTO { ProductId = 99, CategoryId = 10 };

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.AddProductToCategoryAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()), Times.Never);
        _productCategoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddProductToCategoryAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateProductCategoryDTO { ProductId = 1, CategoryId = 99 };

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.AddProductToCategoryAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddProductToCategoryAsync_WhenAlreadyAssigned_ShouldReturnConflict()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateProductCategoryDTO { ProductId = 1, CategoryId = 10 };

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>()))
            .ReturnsAsync(true);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.AddProductToCategoryAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _productCategoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
