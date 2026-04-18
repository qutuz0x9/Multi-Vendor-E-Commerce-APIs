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

public class RemoveProductFromCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductCategoryRepository> _productCategoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IProductCategoryService _productCategoryService;

    public RemoveProductFromCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productCategoryRepositoryMock = new Mock<IProductCategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.ProductCategories).Returns(_productCategoryRepositoryMock.Object);

        _productCategoryService = new ProductCategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task RemoveProductFromCategoryAsync_WhenAssignmentExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var productCategory = new ProductCategory
        {
            Id = 1,
            ProductId = 5,
            CategoryId = 10,
            CreatedAt = DateTime.UtcNow
        };

        _productCategoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(productCategory);

        _productCategoryRepositoryMock
            .Setup(r => r.DeleteAsync(productCategory))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.RemoveProductFromCategoryAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);

        _productCategoryRepositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.DeleteAsync(productCategory), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveProductFromCategoryAsync_WhenAssignmentNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _productCategoryRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((ProductCategory?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _productCategoryService.RemoveProductFromCategoryAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _productCategoryRepositoryMock.Verify(r => r.GetByIdAsync(99), Times.Once);
        _productCategoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<ProductCategory>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
