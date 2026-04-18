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

namespace MultiVendorECommerce.Application.Test.ProductServiceTest;

public class GetAllProductsTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;

    public GetAllProductsTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var brand = new Brand { Id = 1, Name = "Nike" };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Air Max", Slug = "air-max", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, IsDeleted = false },
            new() { Id = 2, Name = "Air Force", Slug = "air-force", BrandId = 1, Brand = brand, Status = ProductStatus.Active, CreatedAt = DateTime.UtcNow, IsDeleted = false }
        };

        _productRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProductsExist_ShouldReturnEmptyCollection()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
