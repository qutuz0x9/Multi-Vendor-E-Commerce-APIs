using AutoMapper;
using FluentAssertions;
using Moq;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.BrandServiceTest;

public class CreateBrandTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly IMapper _mapper;
    private readonly IBrandService _brandService;

    public CreateBrandTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _brandRepositoryMock = new Mock<IBrandRepository>();
        _mapper = MapperTestHelper.GetMapper();

        _unitOfWorkMock.Setup(u => u.Brands).Returns(_brandRepositoryMock.Object);

        _brandService = new BrandService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnCreatedBrand()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateBrandDTO { Name = "Nike" };

        _brandRepositoryMock
            .Setup(r => r.GetBrandByNameAsync(request.Name))
            .ReturnsAsync((Brand?)null);

        _brandRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Brand>()))
            .ReturnsAsync((Brand b) => b);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        result.Value.Status.Should().Be(BrandStatus.Active);
        result.Value.Slug.Should().Be("nike");

        _brandRepositoryMock.Verify(r => r.GetBrandByNameAsync(request.Name), Times.Once);
        _brandRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnNotFound()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new CreateBrandDTO { Name = "Nike" };
        var existingBrand = new Brand { Id = 1, Name = "Nike", NormalizedName = "NIKE" };

        _brandRepositoryMock
            .Setup(r => r.GetBrandByNameAsync(request.Name))
            .ReturnsAsync(existingBrand);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _brandService.CreateAsync(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);

        _brandRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
