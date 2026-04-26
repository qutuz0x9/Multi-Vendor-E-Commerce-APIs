using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class CartItemService(IUnitOfWork unitOfWork, IMapper mapper) : ICartItemService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IEnumerable<CartItemDTO>>> GetAllAsync()
    {
        var items = await _unitOfWork.CartItems.GetAllAsync();
        return Result<IEnumerable<CartItemDTO>>.Success(_mapper.Map<IEnumerable<CartItemDTO>>(items));
    }

    public async Task<Result<CartItemDTO>> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (item is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart item not found."), 404);

        return Result<CartItemDTO>.Success(_mapper.Map<CartItemDTO>(item));
    }

    public async Task<Result<IEnumerable<CartItemDTO>>> GetMyCartItemsAsync(Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<IEnumerable<CartItemDTO>>.Failure(Error.Forbidden("Only customers can view cart items."), 403);

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
            return Result<IEnumerable<CartItemDTO>>.Failure(Error.NotFound("Cart session not found."), 404);

        var items = await _unitOfWork.CartItems.GetItemsByCartAsync(cartSession.Id);
        return Result<IEnumerable<CartItemDTO>>.Success(_mapper.Map<IEnumerable<CartItemDTO>>(items));
    }

    public async Task<Result<CartItemDTO>> UpdateAsync(int id, Guid userId, UpdateCartItemDTO request)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CartItemDTO>.Failure(Error.Forbidden("Only customers can update cart items."), 403);

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (cartItem is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart item not found."), 404);

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null || cartItem.CartSessionId != cartSession.Id)
            return Result<CartItemDTO>.Failure(Error.Forbidden("You do not have access to this cart item."), 403);

        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(cartItem.VendorOfferId);
        if (inventory is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Inventory for this offer not found."), 404);

        var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;
        if (availableQuantity < request.Quantity)
            return Result<CartItemDTO>.Failure(Error.Validation($"Insufficient stock. Only {availableQuantity} unit(s) available."), 400);

        cartItem.Quantity = request.Quantity;
        cartItem.ModifiedAt = DateTime.UtcNow;
        await _unitOfWork.CartItems.UpdateAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        return Result<CartItemDTO>.Success(_mapper.Map<CartItemDTO>(cartItem));
    }

    public async Task<Result> RemoveItemAsync(int id, Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result.Failure(Error.Forbidden("Only customers can remove cart items."), 403);

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (cartItem is null)
            return Result.Failure(Error.NotFound("Cart item not found."), 404);

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null || cartItem.CartSessionId != cartSession.Id)
            return Result.Failure(Error.Forbidden("You do not have access to this cart item."), 403);

        await _unitOfWork.CartItems.DeleteAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(204);
    }

    public async Task<Result<CartItemDTO>> AddItemAsync(Guid userId, AddCartItemDTO request)
    {
        // 1) Verify the user has a Customer profile
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CartItemDTO>.Failure(Error.Forbidden("Only customers can add items to a cart."), 403);

        // 2) Verify the customer has an active CartSession
        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart session not found for this customer."), 404);

        // 3) Verify the VendorOffer exists
        var vendorOffer = await _unitOfWork.VendorOffers.GetByIdAsync(request.VendorOfferId);
        if (vendorOffer is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Vendor offer not found."), 404);

        // 4) Verify the VendorOffer is active
        if (vendorOffer.Staus != VendorOfferStatus.Active)
            return Result<CartItemDTO>.Failure(Error.Validation("This offer is not currently available."), 400);

        // 5) Verify the inventory exists
        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(request.VendorOfferId);
        if (inventory is null)
            return Result<CartItemDTO>.Failure(Error.NotFound("Inventory for this offer not found."), 404);

        // 6) Verify there is sufficient stock
        var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;
        if (availableQuantity < request.Quantity)
            return Result<CartItemDTO>.Failure(Error.Validation($"Insufficient stock. Only {availableQuantity} unit(s) available."), 400);

        // 7) Add or update the CartItem (prevent duplicate rows)
        var existingItem = await _unitOfWork.CartItems.GetCartItemByVendorOfferAsync(cartSession.Id, request.VendorOfferId);
        CartItem cartItem;

        if (existingItem is not null)
        {
            existingItem.Quantity = request.Quantity;
            await _unitOfWork.CartItems.UpdateAsync(existingItem);
            cartItem = existingItem;
        }
        else
        {
            cartItem = new CartItem
            {
                CartSessionId = cartSession.Id,
                VendorOfferId = request.VendorOfferId,
                Quantity = request.Quantity
            };
            await _unitOfWork.CartItems.AddAsync(cartItem);
        }

        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<CartItemDTO>(cartItem);
        return Result<CartItemDTO>.Success(dto);
    }
}
