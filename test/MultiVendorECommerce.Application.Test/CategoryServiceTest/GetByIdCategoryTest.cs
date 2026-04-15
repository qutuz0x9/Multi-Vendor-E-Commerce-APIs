using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.CategoryServiceTest;

public class GetByIdCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICategoryService _categoryService;

    public GetByIdCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            Slug = "electronics",
            Status = CategoryStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.GetByIdAsync(1);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be(category.Name);
        result.Value.Description.Should().Be(category.Description);
        result.Value.Slug.Should().Be(category.Slug);
        result.Value.Status.Should().Be(category.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.GetByIdAsync(99);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedCategory = new Category
        {
            Id = 2,
            Name = "Archived",
            Description = "Archived category",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedCategory);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.GetByIdAsync(2);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }
}
