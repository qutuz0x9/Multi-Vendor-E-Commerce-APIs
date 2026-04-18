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

public class CreateCategoryTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICategoryService _categoryService;

    public CreateCategoryTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnCreatedCategory()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateCategoryDTO
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        _categoryRepositoryMock
            .Setup(r => r.GetCategoryByNameAsync(request.Name))
            .ReturnsAsync((Category?)null);

        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        result.Value.Description.Should().Be(request.Description);
        result.Value.Status.Should().Be(CategoryStatus.Active);
        result.Value.Slug.Should().Be("electronics");

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _categoryRepositoryMock.Verify(r => r.GetCategoryByNameAsync(request.Name), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnBadRequest()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateCategoryDTO
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        _categoryRepositoryMock
            .Setup(r => r.GetCategoryByNameAsync(request.Name))
            .ReturnsAsync(new Category { Id = 1, Name = request.Name });

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetCategoryByNameAsync(request.Name), Times.Once);
    }
}
