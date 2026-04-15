using AutoMapper;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class BrandService(IUnitOfWork unitOfWork, IMapper mapper) : IBrandService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<BrandDTO>> GetByIdAsync(int id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
            return Result<BrandDTO>.Failure(Error.NotFound("Brand not found."));

        return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand));
    }

    public async Task<Result<IEnumerable<BrandDTO>>> GetAllAsync()
    {
        var brands = await _unitOfWork.Brands.GetAllAsync();
        return Result<IEnumerable<BrandDTO>>.Success(_mapper.Map<IEnumerable<BrandDTO>>(brands));
    }

    public async Task<Result<BrandDTO>> CreateAsync(CreateBrandDTO request)
    {
        var exists = await _unitOfWork.Brands.GetBrandByNameAsync(request.Name) != null;
        if (exists)
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 409);

        var brand = new Brand
        {
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant(),
            Slug = SlugHelper.GenerateSlug(request.Name),
            Status = BrandStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Brands.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand), 201);
    }

    public async Task<Result<BrandDTO>> UpdateAsync(int id, UpdateBrandDTO request)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
            return Result<BrandDTO>.Failure(Error.NotFound("Brand not found."));

        var nameConflict = await _unitOfWork.Brands.GetBrandByNameAsync(request.Name) != null && brand.NormalizedName != request.Name.ToUpperInvariant();
        if (nameConflict)
            return Result<BrandDTO>.Failure(Error.Validation("A brand with this name already exists."), 409);

        brand.Name = request.Name;
        brand.NormalizedName = request.Name.ToUpperInvariant();
        brand.Slug = SlugHelper.GenerateSlug(request.Name);
        brand.Status = request.Status;
        brand.ModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Brands.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return Result<BrandDTO>.Success(_mapper.Map<BrandDTO>(brand));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand is null || brand.IsDeleted)
            return Result.Failure(Error.NotFound("Brand not found."));

        await _unitOfWork.Brands.DeleteAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
