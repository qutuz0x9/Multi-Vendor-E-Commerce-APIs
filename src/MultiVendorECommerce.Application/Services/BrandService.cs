using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class BrandService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<BrandService> logger) : IBrandService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<BrandService> _logger = logger;

    public async Task<Result<BrandDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching brand with ID {BrandId}", id);
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
        {
            _logger.LogWarning("Brand with ID {BrandId} not found", id);
            return Result<BrandDTO>.Failure(Error.NotFound("Brand not found."));
        }

        return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand));
    }

    public async Task<Result<IEnumerable<BrandDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all brands");
        var brands = await _unitOfWork.Brands.GetAllAsync();
        return Result<IEnumerable<BrandDTO>>.Success(_mapper.Map<IEnumerable<BrandDTO>>(brands));
    }

    public async Task<Result<BrandDTO>> CreateAsync(CreateBrandDTO request)
    {
        _logger.LogInformation("Creating brand with name {BrandName}", request.Name);
        var exists = await _unitOfWork.Brands.GetBrandByNameAsync(request.Name) != null;
        if (exists)
        {
            _logger.LogWarning("Brand creation failed: name {BrandName} already exists", request.Name);
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 400);
        }

        var brand = new Brand
        {
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant(),
            Slug = SlugHelper.GenerateSlug(request.Name),
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        try
        {
            await _unitOfWork.Brands.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("Brand creation failed on DB constraint: name {BrandName} already exists", request.Name);
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 409);
        }

        _logger.LogInformation("Brand {BrandId} created successfully", brand.Id);
        return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand), 201);
    }

    public async Task<Result<BrandDTO>> UpdateAsync(int id, UpdateBrandDTO request)
    {
        _logger.LogInformation("Updating brand {BrandId}", id);
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
        {
            _logger.LogWarning("Update failed: brand {BrandId} not found", id);
            return Result<BrandDTO>.Failure(Error.NotFound("Brand not found."));
        }

        var existing = await _unitOfWork.Brands.GetBrandByNameAsync(request.Name);
        if (existing != null && existing.Id != id)
        {
            _logger.LogWarning("Update failed: brand name {BrandName} already taken by brand {ExistingBrandId}", request.Name, existing.Id);
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 409);
        }

        brand.Name = request.Name;
        brand.NormalizedName = request.Name.ToUpperInvariant();
        brand.Slug = SlugHelper.GenerateSlug(request.Name);
        brand.Status = request.Status;
        brand.ModifiedAt = DateTime.UtcNow;
        try
        {
            await _unitOfWork.Brands.UpdateAsync(brand);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Brand {BrandId} updated successfully", id);
            return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand));
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("Brand update failed on DB constraint for brand {BrandId}", id);
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 409);
        }

    }

    public async Task<Result> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting brand {BrandId}", id);
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
        {
            _logger.LogWarning("Delete failed: brand {BrandId} not found", id);
            return Result.Failure(Error.NotFound("Brand not found."));
        }

        await _unitOfWork.Brands.DeleteAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} deleted successfully", id);
        return Result.Success();
    }
}
