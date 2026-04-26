using System.Security.Claims;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Shared.Logging;
using AutoMapper;

namespace MultiVendorECommerce.Application.Services;

public class VendorOfferService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<VendorOfferService> logger) : IVendorOfferService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<VendorOfferService> _logger = logger;

    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all vendor offers");
        var offers = await _unitOfWork.VendorOffers.GetAllAsync();
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }

    public async Task<Result<VendorOfferDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching vendor offer {OfferId}", id);
        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
        {
            _logger.LogWarning("Vendor offer {OfferId} not found", id);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor offer not found."), 400);
        }
        var offerDto = _mapper.Map<VendorOfferDTO>(offer);
        return Result<VendorOfferDTO>.Success(offerDto);

    }
    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByVendorAsync(Guid userId)
    {
        _logger.LogDebug("Fetching offers for vendor user {UserId}", userId);
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
        {
            _logger.LogWarning("GetOffersByVendor failed: vendor not found for user {UserId}", userId);
            return Result<IEnumerable<VendorOfferDTO>>.Failure(Error.NotFound("Vendor not found."), 400);
        }
        var offers = await _unitOfWork.VendorOffers.GetOffersByVendorAsync(vendor.Id);
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }
    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByProductAsync(int productId)
    {
        _logger.LogDebug("Fetching offers for product {ProductId}", productId);
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("GetOffersByProduct failed: product {ProductId} not found", productId);
            return Result<IEnumerable<VendorOfferDTO>>.Failure(Error.NotFound("Product not found."), 400);
        }
        var offers = await _unitOfWork.VendorOffers.GetOffersByProductAsync(productId);
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }
    public async Task<Result<VendorOfferDTO>> CreateAsync(Guid userId, CreateVendorOfferDTO request)
    {
        _logger.LogInformation("Creating vendor offer for user {UserId}: product {ProductId}", userId, request.ProductId);
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
        {
            _logger.LogWarning("Create vendor offer failed: vendor not found for user {UserId}", userId);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor not found."), 400);
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Create vendor offer failed: product {ProductId} not found", request.ProductId);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Product not found."), 400);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var offer = _mapper.Map<VendorOffer>(request);
            offer.VendorId = vendor.Id;
            offer.Staus = VendorOfferStatus.Active;
            await _unitOfWork.VendorOffers.AddAsync(offer);
            await _unitOfWork.SaveChangesAsync(); // flush to generate offer.Id

            var inventory = new Inventory
            {
                VendorOfferId = offer.Id,
                Quantity = request.Quantity,
                ReservedQuantity = 0
            };
            await _unitOfWork.Inventories.AddAsync(inventory);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
            _logger.LogInformation("Vendor offer {OfferId} created for vendor {VendorId}", offer.Id, vendor.Id);
            var offerDto = _mapper.Map<VendorOfferDTO>(offer);
            return Result<VendorOfferDTO>.Success(offerDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    public async Task<Result<VendorOfferDTO>> UpdateAsync(Guid userId, int id, UpdateVendorOfferDTO request)
    {
        _logger.LogInformation("Updating vendor offer {OfferId} for user {UserId}", id, userId);
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
        {
            _logger.LogWarning("Update vendor offer failed: vendor not found for user {UserId}", userId);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor not found."), 400);
        }

        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
        {
            _logger.LogWarning("Update vendor offer failed: offer {OfferId} not found", id);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor offer not found."), 400);
        }

        if (offer.VendorId != vendor.Id)
        {
            _logger.LogWarning("Update vendor offer forbidden: offer {OfferId} does not belong to vendor {VendorId}", id, vendor.Id);
            return Result<VendorOfferDTO>.Failure(Error.Forbidden("You do not own this offer."), 403);
        }

        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(id);
        if (inventory == null)
        {
            _logger.LogWarning("Update vendor offer failed: no inventory for offer {OfferId}", id);
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Inventory for offer not found."), 400);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _mapper.Map(request, offer);
            await _unitOfWork.VendorOffers.UpdateAsync(offer);

            inventory.Quantity = request.Quantity;
            await _unitOfWork.Inventories.UpdateAsync(inventory);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Vendor offer {OfferId} updated successfully", id);
            var offerDto = _mapper.Map<VendorOfferDTO>(offer);
            return Result<VendorOfferDTO>.Success(offerDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    public async Task<Result> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting vendor offer {OfferId}", id);
        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
        {
            _logger.LogWarning("Delete vendor offer failed: offer {OfferId} not found", id);
            return Result.Failure(Error.NotFound("Vendor offer not found."), 400);
        }
        await _unitOfWork.VendorOffers.DeleteAsync(offer);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Vendor offer {OfferId} deleted successfully", id);
        return Result.Success();
    }

}