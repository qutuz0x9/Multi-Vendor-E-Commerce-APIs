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

namespace MultiVendorECommerce.Application.Test.CategoryServiceTest;

public class GetAllCategoriesTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ICategoryService _categoryService;

    public GetAllCategoriesTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_WhenCategoriesExist_ShouldReturnMappedDTOs()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronics", Slug = "electronics", Status = CategoryStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Clothing",    Description = "Clothing",    Slug = "clothing",    Status = CategoryStatus.Active, CreatedAt = DateTime.UtcNow }
        };
        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Select(c => c.Name).Should().BeEquivalentTo(["Electronics", "Clothing"]);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCategoriesExist_ShouldReturnEmptyList()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync([]);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _categoryService.GetAllAsync();

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}
