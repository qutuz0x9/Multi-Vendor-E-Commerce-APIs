using AutoMapper;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<ProductCategoryService> logger) : IProductCategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<ProductCategoryService> _logger = logger;

    public async Task<Result<IEnumerable<ProductCategoryDTO>>> GetCategoriesByProductAsync(int productId)
    {
        _logger.LogDebug("Fetching categories for product {ProductId}", productId);
        var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            _logger.LogWarning("GetCategoriesByProduct failed: product {ProductId} not found", productId);
            return Result<IEnumerable<ProductCategoryDTO>>.Failure(Error.NotFound("Product not found."));
        }

        var productCategories = await _unitOfWork.ProductCategories.GetCategoriesByProductAsync(productId);
        return Result<IEnumerable<ProductCategoryDTO>>.Success(_mapper.Map<IEnumerable<ProductCategoryDTO>>(productCategories));
    }

    public async Task<Result<IEnumerable<ProductCategoryDTO>>> GetProductsByCategoryAsync(int categoryId)
    {
        _logger.LogDebug("Fetching products for category {CategoryId}", categoryId);
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("GetProductsByCategory failed: category {CategoryId} not found", categoryId);
            return Result<IEnumerable<ProductCategoryDTO>>.Failure(Error.NotFound("Category not found."));
        }

        var productCategories = await _unitOfWork.ProductCategories.GetProductsByCategory(categoryId);
        return Result<IEnumerable<ProductCategoryDTO>>.Success(_mapper.Map<IEnumerable<ProductCategoryDTO>>(productCategories));
    }

    public async Task<Result<ProductCategoryDTO>> AddProductToCategoryAsync(CreateProductCategoryDTO request)
    {
        _logger.LogInformation("Adding product {ProductId} to category {CategoryId}", request.ProductId, request.CategoryId);
        var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == request.ProductId);
        if (!productExists)
        {
            _logger.LogWarning("AddProductToCategory failed: product {ProductId} not found", request.ProductId);
            return Result<ProductCategoryDTO>.Failure(Error.NotFound("Product not found."));
        }

        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("AddProductToCategory failed: category {CategoryId} not found", request.CategoryId);
            return Result<ProductCategoryDTO>.Failure(Error.NotFound("Category not found."));
        }

        var alreadyAssigned = await _unitOfWork.ProductCategories.AnyAsync(
            pc => pc.ProductId == request.ProductId && pc.CategoryId == request.CategoryId);
        if (alreadyAssigned)
        {
            _logger.LogWarning("AddProductToCategory failed: product {ProductId} already in category {CategoryId}", request.ProductId, request.CategoryId);
            return Result<ProductCategoryDTO>.Failure(Error.Validation("Product is already assigned to this category."), 409);
        }

        var productCategory = new ProductCategory
        {
            ProductId = request.ProductId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductCategories.AddAsync(productCategory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} added to category {CategoryId} (assignment {AssignmentId})", request.ProductId, request.CategoryId, productCategory.Id);
        return Result<ProductCategoryDTO>.Success(_mapper.Map<ProductCategoryDTO>(productCategory), 201);
    }

    public async Task<Result> RemoveProductFromCategoryAsync(int id)
    {
        _logger.LogInformation("Removing product-category assignment {AssignmentId}", id);
        var productCategory = await _unitOfWork.ProductCategories.GetByIdAsync(id);
        if (productCategory is null)
        {
            _logger.LogWarning("RemoveProductFromCategory failed: assignment {AssignmentId} not found", id);
            return Result.Failure(Error.NotFound("Product-category assignment not found."));
        }

        await _unitOfWork.ProductCategories.DeleteAsync(productCategory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product-category assignment {AssignmentId} removed successfully", id);
        return Result.Success();
    }
}
