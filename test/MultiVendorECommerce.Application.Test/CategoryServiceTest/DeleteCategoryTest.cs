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
namespace MultiVendorECommerce.Application.Test.CategoryServiceTest;

public class DeleteCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAppLogger<CategoryService>> _loggerMock;
    private readonly ICategoryService _categoryService;

    public DeleteCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();
        _loggerMock = new Mock<IAppLogger<CategoryService>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            IsDeleted = false
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(r => r.DeleteAsync(category))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.DeleteAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);

        _categoryRepositoryMock.Verify(r => r.DeleteAsync(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.DeleteAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedCategory = new Category
        {
            Id = 2,
            Name = "Archived",
            Description = "Archived category",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-5)
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedCategory);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.DeleteAsync(2);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
