using System.Text.Json;
using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;


namespace MultiVendorECommerce.Application.Services;

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<ProductService> logger) : IProductService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<ProductService> _logger = logger;

    public async Task<Result<ProductDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching product {ProductId}", id);
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null || product.IsDeleted)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return Result<ProductDTO>.Failure(Error.NotFound("Product not found."));
        }
        var productDto = _mapper.Map<ProductDTO>(product);
        productDto.Categories = product.ProductCategories.Select(pc => pc.Category != null ? pc.Category.Name : string.Empty);
        productDto.BrandName = product.Brand != null ? product.Brand.Name : string.Empty;

        return Result<ProductDTO>.Success(productDto);
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all products");
        var products = await _unitOfWork.Products.GetAllAsync();
        var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
        foreach (var productDto in productDtos)
        {
            var product = products.FirstOrDefault(p => p.Id == productDto.Id);
            if (product != null)
            {
                productDto.Categories = product.ProductCategories.Select(pc => pc.Category != null ? pc.Category.Name : string.Empty);
                productDto.BrandName = product.Brand != null ? product.Brand.Name : string.Empty;
            }
        }
        return Result<IEnumerable<ProductDTO>>.Success(productDtos);
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetProductsByBrandAsync(int brandId)
    {
        _logger.LogDebug("Fetching products for brand {BrandId}", brandId);
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId);
        if (brand is null)
        {
            _logger.LogWarning("GetProductsByBrand failed: brand {BrandId} not found", brandId);
            return Result<IEnumerable<ProductDTO>>.Failure(Error.NotFound("Brand not found."));
        }

        var products = await _unitOfWork.Products.GetProductsByBrandAsync(brandId);
        var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
        foreach (var productDto in productDtos)
        {
            var product = products.FirstOrDefault(p => p.Id == productDto.Id);
            if (product != null)
            {
                productDto.Categories = product.ProductCategories.Select(pc => pc.Category != null ? pc.Category.Name : string.Empty);
                productDto.BrandName = product.Brand != null ? product.Brand.Name : string.Empty;
            }
        }
        return Result<IEnumerable<ProductDTO>>.Success(productDtos);
    }

    public async Task<Result<IEnumerable<ProductDTO>>> GetProductsByCategoryAsync(int categoryId)
    {
        _logger.LogDebug("Fetching products for category {CategoryId}", categoryId);
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        if (category is null)
        {
            _logger.LogWarning("GetProductsByCategory failed: category {CategoryId} not found", categoryId);
            return Result<IEnumerable<ProductDTO>>.Failure(Error.NotFound("Category not found."));
        }

        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
        foreach (var productDto in productDtos)
        {
            var product = products.FirstOrDefault(p => p.Id == productDto.Id);
            if (product != null)
            {
                productDto.Categories = product.ProductCategories.Select(pc => pc.Category != null ? pc.Category.Name : string.Empty);
                productDto.BrandName = product.Brand != null ? product.Brand.Name : string.Empty;
            }
        }
        return Result<IEnumerable<ProductDTO>>.Success(productDtos);
    }

    public async Task<Result<ProductDTO>> CreateAsync(CreateProductDTO request)
    {
        _logger.LogInformation("Creating product with name {ProductName}", request.Name);
        // 1) Validate brand exists
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId);
        if (brand is null)
        {
            _logger.LogWarning("Product creation failed: brand {BrandId} not found", request.BrandId);
            return Result<ProductDTO>.Failure(Error.NotFound("Brand not found."));
        }
        // 2) Validate slug uniqueness (optimistic pre-check — DB unique constraint is the definitive guard)
        var slug = SlugHelper.GenerateProductSlug(request.Name, request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null);
        var slugExists = await _unitOfWork.Products.GetProductBySlugAsync(slug);
        if (slugExists is not null)
        {
            _logger.LogWarning("Product creation failed: slug {Slug} already exists", slug);
            return Result<ProductDTO>.Failure(Error.Validation("A product with this name already exists."), 400);
        }
        // 3) Validate categories exist
        var categories = new List<Category>();
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category is null)
            {
                _logger.LogWarning("Product creation failed: category {CategoryId} not found", categoryId);
                return Result<ProductDTO>.Failure(Error.NotFound($"Category with ID {categoryId} not found."));
            }
            categories.Add(category);
        }

        // 4) Atomically persist product + categories inside a transaction.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
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

            // Flush to DB to obtain the generated product.Id;
            // returns false if a concurrent request already committed the same slug.
            var saved = await _unitOfWork.TrySaveChangesAsync();
            if (!saved)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Product creation failed on DB constraint: slug {Slug} already exists", slug);
                return Result<ProductDTO>.Failure(Error.Validation("A product with this name already exists."), 400);
            }

            foreach (var category in categories)
            {
                await _unitOfWork.ProductCategories.AddAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // CommitTransactionAsync calls SaveChangesAsync internally before committing,
            // persisting all category links in the same transaction.
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Product {ProductId} created successfully", product.Id);
            var productDto = _mapper.Map<ProductDTO>(product);
            // categories were validated above, so project names directly — no extra DB round-trip needed.
            productDto.Categories = categories.Select(c => c.Name);
            productDto.BrandName = brand != null ? brand.Name : string.Empty;
            return Result<ProductDTO>.Success(productDto, 201);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result<ProductDTO>> UpdateAsync(int id, UpdateProductDTO request)
    {
        _logger.LogInformation("Updating product {ProductId}", id);
        // 1) Validate product exists and is not deleted
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null || product.IsDeleted)
        {
            _logger.LogWarning("Product update failed: product {ProductId} not found", id);
            return Result<ProductDTO>.Failure(Error.NotFound("Product not found."));
        }
        // 2) Validate brand exists
        var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId);
        if (brand is null)
        {
            _logger.LogWarning("Product update failed: brand {BrandId} not found", request.BrandId);
            return Result<ProductDTO>.Failure(Error.NotFound("Brand not found."));
        }
        // 3) Validate slug uniqueness (optimistic pre-check)
        var slug = SlugHelper.GenerateProductSlug(request.Name, request.Feature.HasValue ? JsonDocument.Parse(request.Feature.Value.GetRawText()) : null);
        var existingBySlug = await _unitOfWork.Products.GetProductBySlugAsync(slug);
        if (existingBySlug is not null && existingBySlug.Id != id)
        {
            _logger.LogWarning("Product update failed: slug {Slug} already taken by product {ExistingProductId}", slug, existingBySlug.Id);
            return Result<ProductDTO>.Failure(Error.Validation("A product with this name already exists."), 409);
        }
        // 4) Validate categories exist
        var categories = new List<Category>();
        foreach (var categoryId in request.CategoryIds)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category is null)
            {
                _logger.LogWarning("Product update failed: category {CategoryId} not found", categoryId);
                return Result<ProductDTO>.Failure(Error.NotFound($"Category with ID {categoryId} not found."));
            }
            categories.Add(category);
        }

        // 5) Atomically replace categories and update product inside a transaction.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
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
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Product {ProductId} updated successfully", id);
            var productDto = _mapper.Map<ProductDTO>(product);
            productDto.Categories = categories.Select(c => c.Name);
            productDto.BrandName = brand != null ? brand.Name : string.Empty;
            return Result<ProductDTO>.Success(productDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null || product.IsDeleted)
        {
            _logger.LogWarning("Product delete failed: product {ProductId} not found", id);
            return Result.Failure(Error.NotFound("Product not found."));
        }

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted successfully", id);
        return Result.Success();
    }
}