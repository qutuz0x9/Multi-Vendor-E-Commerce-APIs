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

namespace MultiVendorECommerce.Application.Test.ProductCategoryServiceTest;

public class GetCategoriesByProductTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IProductCategoryService _productCategoryService;

    public GetCategoriesByProductTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        _productCategoryService = new ProductCategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetCategoriesByProductAsync_WhenProductExists_ShouldReturnCategories()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var productCategories = new List<ProductCategory>
        {
            new ProductCategory
            {
                Id = 1,
                ProductId = 1,
                CategoryId = 10,
                CreatedAt = DateTime.UtcNow,
                Product = new Product { Id = 1, Name = "Laptop", Slug = "laptop", Description = "" },
                Category = new Category { Id = 10, Name = "Electronics", NormalizedName = "ELECTRONICS", Description = "" }
            },
            new ProductCategory
            {
                Id = 2,
                ProductId = 1,
                CategoryId = 11,
                CreatedAt = DateTime.UtcNow,
                Product = new Product { Id = 1, Name = "Laptop", Slug = "laptop", Description = "" },
                Category = new Category { Id = 11, Name = "Computers", NormalizedName = "COMPUTERS", Description = "" }
            }
        };

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.GetCategoriesByProductAsync(1))
            .ReturnsAsync(productCategories);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetCategoriesByProductAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);

        _productRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.GetCategoriesByProductAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesByProductAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetCategoriesByProductAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.GetCategoriesByProductAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCategoriesByProductAsync_WhenProductHasNoCategories_ShouldReturnEmptyCollection()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        _productCategoryRepositoryMock
            .Setup(r => r.GetCategoriesByProductAsync(1))
            .ReturnsAsync(new List<ProductCategory>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.GetCategoriesByProductAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
