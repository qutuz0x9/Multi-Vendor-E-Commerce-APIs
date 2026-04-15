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

public class UpdateCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICategoryService _categoryService;

    public UpdateCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedCategory()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var existing = new Category
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old description",
            Slug = "old-name",
            Status = CategoryStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var request = new UpdateCategoryDTO
        {
            Name = "New Name",
            Description = "New description",
            Status = CategoryStatus.Inactive
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _categoryRepositoryMock
            .Setup(r => r.GetCategoryByNameAsync(request.Name))
            .ReturnsAsync((Category?)null);

        _categoryRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        result.Value.Description.Should().Be(request.Description);
        result.Value.Status.Should().Be(request.Status);
        result.Value.Slug.Should().Be("new-name");

        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        var request = new UpdateCategoryDTO { Name = "Name", Description = "Desc", Status = CategoryStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.UpdateAsync(99, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryIsDeleted_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var deletedCategory = new Category { Id = 2, Name = "Archived", Description = "Archived", IsDeleted = true };
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(deletedCategory);

        var request = new UpdateCategoryDTO { Name = "Name", Description = "Desc", Status = CategoryStatus.Active };

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.UpdateAsync(2, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ShouldReturnConflict()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new UpdateCategoryDTO
        {
            Name = "Clothing",
            Description = "Clothing",
            Status = CategoryStatus.Active
        };

        var existing = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronics",
            Slug = "electronics",
            Status = CategoryStatus.Active,
            IsDeleted = false
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _categoryRepositoryMock
            .Setup(r => r.GetCategoryByNameAsync(request.Name))
            .ReturnsAsync(new Category { Id = 2, Name = request.Name });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.UpdateAsync(1, request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(409);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
