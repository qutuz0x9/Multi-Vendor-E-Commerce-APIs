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

using MultiVendorECommerce.Shared.Logging;
namespace MultiVendorECommerce.Application.Test.ProductCategoryServiceTest;

public class GetProductsByCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<ProductCategoryService>> _loggerMock;
    private readonly IProductCategoryService _productCategoryService;

    public GetProductsByCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<ProductCategoryService>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        _productCategoryService = new ProductCategoryService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryExists_ShouldReturnProducts()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var productCategories = new List<ProductCategory>
        {
            new ProductCategory
            {
                Id = 1,
                ProductId = 5,
                CategoryId = 10,
                CreatedAt = DateTime.UtcNow,
                Product = new Product { Id = 5, Name = "Laptop", Slug = "laptop", Description = "" },
                Category = new Category { Id = 10, Name = "Electronics", NormalizedName = "ELECTRONICS", Description = "" }
            },
            new ProductCategory
            {
                Id = 2,
                ProductId = 6,
                CategoryId = 10,
                CreatedAt = DateTime.UtcNow,
                Product = new Product { Id = 6, Name = "Phone", Slug = "phone", Description = "" },
                Category = new Category { Id = 10, Name = "Electronics", NormalizedName = "ELECTRONICS", Description = "" }
            }
        };

        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.GetProductsByCategory(10))
            .ReturnsAsync(productCategories);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetProductsByCategoryAsync(10);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);

        _categoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.GetProductsByCategory(10), Times.Once);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetProductsByCategoryAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.GetProductsByCategory(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_WhenCategoryHasNoProducts_ShouldReturnEmptyCollection()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.GetProductsByCategory(10))
            .ReturnsAsync(new List<ProductCategory>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetProductsByCategoryAsync(10);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
