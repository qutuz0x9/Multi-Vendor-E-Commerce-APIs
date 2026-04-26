using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<CategoryService> logger) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<CategoryService> _logger = logger;

    public async Task<Result<CategoryDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching category with ID {CategoryId}", id);
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found", id);
            return Result<CategoryDTO>.Failure(Error.NotFound("Category not found."));
        }

        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category));
    }

    public async Task<Result<IEnumerable<CategoryDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all categories");
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return Result<IEnumerable<CategoryDTO>>.Success(_mapper.Map<IEnumerable<CategoryDTO>>(categories));
    }

    public async Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO request)
    {
        _logger.LogInformation("Creating category with name {CategoryName}", request.Name);
        var categoryExists = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Name);
        if (categoryExists is not null)
        {
            _logger.LogWarning("Category creation failed: name {CategoryName} already exists", request.Name);
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            NormalizedName = request.Name.ToUpperInvariant(),
            Slug = SlugHelper.GenerateSlug(request.Name),
            Status = CategoryStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Categories.AddAsync(category);
        if (!await _unitOfWork.TrySaveChangesAsync())
        {
            _logger.LogWarning("Category creation failed on DB constraint: name {CategoryName} already exists", request.Name);
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);
        }

        _logger.LogInformation("Category {CategoryId} created successfully", category.Id);
        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category), 201);
    }

    public async Task<Result<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request)
    {
        _logger.LogInformation("Updating category {CategoryId}", id);
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
        {
            _logger.LogWarning("Update failed: category {CategoryId} not found", id);
            return Result<CategoryDTO>.Failure(Error.NotFound("Category not found."));
        }

        var categoryExists = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Name);
        if (categoryExists is not null && categoryExists.Id != id)
        {
            _logger.LogWarning("Update failed: category name {CategoryName} already taken by category {ExistingCategoryId}", request.Name, categoryExists.Id);
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.NormalizedName = request.Name.ToUpperInvariant();
        category.Slug = SlugHelper.GenerateSlug(request.Name);
        category.Status = request.Status;
        category.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category);
        if (!await _unitOfWork.TrySaveChangesAsync())
        {
            _logger.LogWarning("Category update failed on DB constraint for category {CategoryId}", id);
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);
        }

        _logger.LogInformation("Category {CategoryId} updated successfully", id);
        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting category {CategoryId}", id);
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
        {
            _logger.LogWarning("Delete failed: category {CategoryId} not found", id);
            return Result.Failure(Error.NotFound("Category not found."));
        }

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} deleted successfully", id);
        return Result.Success();
    }
}
