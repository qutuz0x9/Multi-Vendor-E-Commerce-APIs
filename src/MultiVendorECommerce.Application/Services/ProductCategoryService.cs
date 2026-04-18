using AutoMapper;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper) : IProductCategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IEnumerable<ProductCategoryDTO>>> GetCategoriesByProductAsync(int productId)
    {
        var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
            return Result<IEnumerable<ProductCategoryDTO>>.Failure(Error.NotFound("Product not found."));

        var productCategories = await _unitOfWork.ProductCategories.GetCategoriesByProductAsync(productId);
        return Result<IEnumerable<ProductCategoryDTO>>.Success(_mapper.Map<IEnumerable<ProductCategoryDTO>>(productCategories));
    }

    public async Task<Result<IEnumerable<ProductCategoryDTO>>> GetProductsByCategoryAsync(int categoryId)
    {
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
            return Result<IEnumerable<ProductCategoryDTO>>.Failure(Error.NotFound("Category not found."));

        var productCategories = await _unitOfWork.ProductCategories.GetProductsByCategory(categoryId);
        return Result<IEnumerable<ProductCategoryDTO>>.Success(_mapper.Map<IEnumerable<ProductCategoryDTO>>(productCategories));
    }

    public async Task<Result<ProductCategoryDTO>> AddProductToCategoryAsync(CreateProductCategoryDTO request)
    {
        var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == request.ProductId);
        if (!productExists)
            return Result<ProductCategoryDTO>.Failure(Error.NotFound("Product not found."));

        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            return Result<ProductCategoryDTO>.Failure(Error.NotFound("Category not found."));

        var alreadyAssigned = await _unitOfWork.ProductCategories.AnyAsync(
            pc => pc.ProductId == request.ProductId && pc.CategoryId == request.CategoryId);
        if (alreadyAssigned)
            return Result<ProductCategoryDTO>.Failure(Error.Validation("Product is already assigned to this category."), 409);

        var productCategory = new ProductCategory
        {
            ProductId = request.ProductId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductCategories.AddAsync(productCategory);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProductCategoryDTO>.Success(_mapper.Map<ProductCategoryDTO>(productCategory), 201);
    }

    public async Task<Result> RemoveProductFromCategoryAsync(int id)
    {
        var productCategory = await _unitOfWork.ProductCategories.GetByIdAsync(id);
        if (productCategory is null)
            return Result.Failure(Error.NotFound("Product-category assignment not found."));

        await _unitOfWork.ProductCategories.DeleteAsync(productCategory);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
