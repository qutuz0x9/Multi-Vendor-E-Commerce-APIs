using System.Text.Json;
using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper) : IProductService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProductDTO>> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null || product.IsDeleted)
            return Result<ProductDTO>.Failure(Error.NotFound("Product not found."));

        return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(product));
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return Result<IEnumerable<ProductDTO>>.Success(_mapper.Map<IEnumerable<ProductDTO>>(products));
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetProductsByBrandAsync(int brandId)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
        if (brand is null)
            return Result<IEnumerable<ProductDTO>>.Failure(Error.NotFound("Brand not found."));

        var products = await _unitOfWork.Products.GetProductsByBrandAsync(brandId);
        return Result<IEnumerable<ProductDTO>>.Success(_mapper.Map<IEnumerable<ProductDTO>>(products));
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetProductsByCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        if (category is null)
            return Result<IEnumerable<ProductDTO>>.Failure(Error.NotFound("Category not found."));

        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        return Result<IEnumerable<ProductDTO>>.Success(_mapper.Map<IEnumerable<ProductDTO>>(products));
    }

    public async Task<Result<ProductDTO>> CreateAsync(CreateProductDTO request)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId);
        if (brand is null)
            return Result<ProductDTO>.Failure(Error.NotFound("Brand not found."));

        var slug = SlugHelper.GenerateProductSlug(request.Name, request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null);
        var slugExists = await _unitOfWork.Products.GetProductBySlugAsync(slug);
        if (slugExists is not null)
            return Result<ProductDTO>.Failure(Error.Validation("A product with this name already exists."), 409);

        var categories = new List<Category>();
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category is null)
                return Result<ProductDTO>.Failure(Error.NotFound($"Category with ID {categoryId} not found."));
            categories.Add(category);
        }

        var product = new Product
        {
            BrandId = request.BrandId,
            Name = request.Name,
            Description = request.Description,
            Feature = request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null,
            Slug = slug,
            Status = ProductStatus.Drafted,
            CreatedAt = DateTime.UtcNow,
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        foreach (var category in categories)
        {
            var productCategory = new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };
            product.ProductCategories.Add(productCategory);
            await _unitOfWork.ProductCategories.AddAsync(productCategory);
        }

        if (categories.Count > 0)
            await _unitOfWork.SaveChangesAsync();

        return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(product), 201);
    }

    public async Task<Result<ProductDTO>> UpdateAsync(int id, UpdateProductDTO request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
            return Result<ProductDTO>.Failure(Error.NotFound("Product not found."));
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId);
        if (brand is null)
            return Result<ProductDTO>.Failure(Error.NotFound("Brand not found."));

        var slug = SlugHelper.GenerateProductSlug(request.Name, request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null);
        var existingBySlug = await _unitOfWork.Products.GetProductBySlugAsync(slug);
        if (existingBySlug is not null && existingBySlug.Id != id)
            return Result<ProductDTO>.Failure(Error.Validation("A product with this name already exists."), 409);

        var categories = new List<Category>();
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category is null)
                return Result<ProductDTO>.Failure(Error.NotFound($"Category with ID {categoryId} not found."));
            categories.Add(category);
        }

        product.BrandId = request.BrandId;
        product.Brand = brand;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Feature = request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null;
        product.Slug = slug;
        product.Status = request.Status;
        product.ModifiedAt = DateTime.UtcNow;

        var existingCategories = await _unitOfWork.ProductCategories.GetCategoriesByProductAsync(id);
        foreach (var pc in existingCategories)
            await _unitOfWork.ProductCategories.DeleteAsync(pc);

        product.ProductCategories.Clear();
        foreach (var category in categories)
        {
            var productCategory = new ProductCategory
            {
                ProductId = id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };
            product.ProductCategories.Add(productCategory);
            await _unitOfWork.ProductCategories.AddAsync(productCategory);
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProductDTO>.Success(_mapper.Map<ProductDTO>(product));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
            return Result.Failure(Error.NotFound("Product not found."));

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
