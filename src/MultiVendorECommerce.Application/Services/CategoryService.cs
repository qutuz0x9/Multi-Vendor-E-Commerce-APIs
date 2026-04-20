using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CategoryDTO>> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
            return Result<CategoryDTO>.Failure(Error.NotFound("Category not found."));

        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category));
    }

    public async Task<Result<IEnumerable<CategoryDTO>>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return Result<IEnumerable<CategoryDTO>>.Success(_mapper.Map<IEnumerable<CategoryDTO>>(categories));
    }

    public async Task<Result<CategoryDTO>> CreateAsync(CreateCategoryDTO request)
    {
        var categoryExists = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Name);
        if (categoryExists is not null)
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);

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
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);

        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category), 201);
    }

    public async Task<Result<CategoryDTO>> UpdateAsync(int id, UpdateCategoryDTO request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
            return Result<CategoryDTO>.Failure(Error.NotFound("Category not found."));

        var categoryExists = await _unitOfWork.Categories.GetCategoryByNameAsync(request.Name);
        if (categoryExists is not null && categoryExists.Id != id)
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);

        category.Name = request.Name;
        category.Description = request.Description;
        category.NormalizedName = request.Name.ToUpperInvariant();
        category.Slug = SlugHelper.GenerateSlug(request.Name);
        category.Status = request.Status;
        category.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category);
        if (!await _unitOfWork.TrySaveChangesAsync())
            return Result<CategoryDTO>.Failure(Error.Validation("A category with this name already exists."), 400);

        return Result<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(category));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || category.IsDeleted)
            return Result.Failure(Error.NotFound("Category not found."));

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
