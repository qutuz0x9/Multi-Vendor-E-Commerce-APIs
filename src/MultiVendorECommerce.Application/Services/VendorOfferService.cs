using System.Security.Claims;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Domain.Enums;
using AutoMapper;

namespace MultiVendorECommerce.Application.Services;

public class VendorOfferService(IUnitOfWork unitOfWork, IMapper mapper) : IVendorOfferService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetAllAsync()
    {
        var offers = await _unitOfWork.VendorOffers.GetAllAsync();
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }

    public async Task<Result<VendorOfferDTO>> GetByIdAsync(int id)
    {
        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
        {
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor offer not found."), 400);
        }
        var offerDto = _mapper.Map<VendorOfferDTO>(offer);
        return Result<VendorOfferDTO>.Success(offerDto);

    }
    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByVendorAsync(Guid userId)
    {
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
        {
            return Result<IEnumerable<VendorOfferDTO>>.Failure(Error.NotFound("Vendor not found."), 400);
        }
        var offers = await _unitOfWork.VendorOffers.GetOffersByVendorAsync(vendor.Id);
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }
    public async Task<Result<IEnumerable<VendorOfferDTO>>> GetOffersByProductAsync(int productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
        {
            return Result<IEnumerable<VendorOfferDTO>>.Failure(Error.NotFound("Product not found."), 400);
        }
        var offers = await _unitOfWork.VendorOffers.GetOffersByProductAsync(productId);
        var offerDtos = _mapper.Map<IEnumerable<VendorOfferDTO>>(offers);
        return Result<IEnumerable<VendorOfferDTO>>.Success(offerDtos);
    }
    public async Task<Result<VendorOfferDTO>> CreateAsync(Guid userId, CreateVendorOfferDTO request)
    {
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor not found."), 400);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Product not found."), 400);

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
        var vendor = await _unitOfWork.Vendors.GetVendorByIdAsync(userId);
        if (vendor == null)
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor not found."), 400);

        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Vendor offer not found."), 400);

        if (offer.VendorId != vendor.Id)
            return Result<VendorOfferDTO>.Failure(Error.Forbidden("You do not own this offer."), 403);

        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(id);
        if (inventory == null)
            return Result<VendorOfferDTO>.Failure(Error.NotFound("Inventory for offer not found."), 400);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _mapper.Map(request, offer);
            await _unitOfWork.VendorOffers.UpdateAsync(offer);

            inventory.Quantity = request.Quantity;
            await _unitOfWork.Inventories.UpdateAsync(inventory);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

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
        var offer = await _unitOfWork.VendorOffers.GetByIdAsync(id);
        if (offer == null)
        {
            return Result.Failure(Error.NotFound("Vendor offer not found."), 400);
        }
        await _unitOfWork.VendorOffers.DeleteAsync(offer);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

}