using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class CartItemService(IUnitOfWork unitOfWork, IMapper mapper, IAppLogger<CartItemService> logger) : ICartItemService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAppLogger<CartItemService> _logger = logger;

    public async Task<Result<IEnumerable<CartItemDTO>>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all cart items");
        var items = await _unitOfWork.CartItems.GetAllAsync();
        return Result<IEnumerable<CartItemDTO>>.Success(_mapper.Map<IEnumerable<CartItemDTO>>(items));
    }

    public async Task<Result<CartItemDTO>> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching cart item {CartItemId}", id);
        var item = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (item is null)
        {
            _logger.LogWarning("Cart item {CartItemId} not found", id);
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart item not found."), 404);
        }

        return Result<CartItemDTO>.Success(_mapper.Map<CartItemDTO>(item));
    }

    public async Task<Result<IEnumerable<CartItemDTO>>> GetMyCartItemsAsync(Guid userId)
    {
        _logger.LogDebug("Fetching cart items for user {UserId}", userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Cart items fetch failed: no customer profile for user {UserId}", userId);
            return Result<IEnumerable<CartItemDTO>>.Failure(Error.Forbidden("Only customers can view cart items."), 403);
        }

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
        {
            _logger.LogWarning("Cart items fetch failed: no active cart session for customer {CustomerId}", customer.Id);
            return Result<IEnumerable<CartItemDTO>>.Failure(Error.NotFound("Cart session not found."), 404);
        }

        var items = await _unitOfWork.CartItems.GetItemsByCartAsync(cartSession.Id);
        return Result<IEnumerable<CartItemDTO>>.Success(_mapper.Map<IEnumerable<CartItemDTO>>(items));
    }

    public async Task<Result<CartItemDTO>> UpdateAsync(int id, Guid userId, UpdateCartItemDTO request)
    {
        _logger.LogInformation("Updating cart item {CartItemId} for user {UserId}", id, userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Cart item update failed: no customer profile for user {UserId}", userId);
            return Result<CartItemDTO>.Failure(Error.Forbidden("Only customers can update cart items."), 403);
        }

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (cartItem is null)
        {
            _logger.LogWarning("Cart item update failed: cart item {CartItemId} not found", id);
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart item not found."), 404);
        }

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null || cartItem.CartSessionId != cartSession.Id)
        {
            _logger.LogWarning("Cart item update forbidden: cart item {CartItemId} does not belong to customer {CustomerId}", id, customer.Id);
            return Result<CartItemDTO>.Failure(Error.Forbidden("You do not have access to this cart item."), 403);
        }

        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(cartItem.VendorOfferId);
        if (inventory is null)
        {
            _logger.LogWarning("Cart item update failed: no inventory for vendor offer {VendorOfferId}", cartItem.VendorOfferId);
            return Result<CartItemDTO>.Failure(Error.NotFound("Inventory for this offer not found."), 404);
        }

        var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;
        if (availableQuantity < request.Quantity)
        {
            _logger.LogWarning("Cart item update failed: insufficient stock for offer {VendorOfferId}. Available: {Available}, Requested: {Requested}", cartItem.VendorOfferId, availableQuantity, request.Quantity);
            return Result<CartItemDTO>.Failure(Error.Validation($"Insufficient stock. Only {availableQuantity} unit(s) available."), 400);
        }

        cartItem.Quantity = request.Quantity;
        cartItem.ModifiedAt = DateTime.UtcNow;
        await _unitOfWork.CartItems.UpdateAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cart item {CartItemId} updated successfully", id);
        return Result<CartItemDTO>.Success(_mapper.Map<CartItemDTO>(cartItem));
    }

    public async Task<Result> RemoveItemAsync(int id, Guid userId)
    {
        _logger.LogInformation("Removing cart item {CartItemId} for user {UserId}", id, userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Cart item remove failed: no customer profile for user {UserId}", userId);
            return Result.Failure(Error.Forbidden("Only customers can remove cart items."), 403);
        }

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
        if (cartItem is null)
        {
            _logger.LogWarning("Cart item remove failed: cart item {CartItemId} not found", id);
            return Result.Failure(Error.NotFound("Cart item not found."), 404);
        }

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null || cartItem.CartSessionId != cartSession.Id)
        {
            _logger.LogWarning("Cart item remove forbidden: cart item {CartItemId} does not belong to customer {CustomerId}", id, customer.Id);
            return Result.Failure(Error.Forbidden("You do not have access to this cart item."), 403);
        }

        await _unitOfWork.CartItems.DeleteAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cart item {CartItemId} removed successfully", id);
        return Result.Success(204);
    }

    public async Task<Result<CartItemDTO>> AddItemAsync(Guid userId, AddCartItemDTO request)
    {
        _logger.LogInformation("Adding cart item for user {UserId}: offer {VendorOfferId} x{Quantity}", userId, request.VendorOfferId, request.Quantity);
        // 1) Verify the user has a Customer profile
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Add cart item failed: no customer profile for user {UserId}", userId);
            return Result<CartItemDTO>.Failure(Error.Forbidden("Only customers can add items to a cart."), 403);
        }

        // 2) Verify the customer has an active CartSession
        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
        {
            _logger.LogWarning("Add cart item failed: no active cart session for customer {CustomerId}", customer.Id);
            return Result<CartItemDTO>.Failure(Error.NotFound("Cart session not found for this customer."), 404);
        }

        // 3) Verify the VendorOffer exists
        var vendorOffer = await _unitOfWork.VendorOffers.GetByIdAsync(request.VendorOfferId);
        if (vendorOffer is null)
        {
            _logger.LogWarning("Add cart item failed: vendor offer {VendorOfferId} not found", request.VendorOfferId);
            return Result<CartItemDTO>.Failure(Error.NotFound("Vendor offer not found."), 404);
        }

        // 4) Verify the VendorOffer is active
        if (vendorOffer.Staus != VendorOfferStatus.Active)
        {
            _logger.LogWarning("Add cart item failed: vendor offer {VendorOfferId} is not active", request.VendorOfferId);
            return Result<CartItemDTO>.Failure(Error.Validation("This offer is not currently available."), 400);
        }

        // 5) Verify the inventory exists
        var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(request.VendorOfferId);
        if (inventory is null)
        {
            _logger.LogWarning("Add cart item failed: no inventory for vendor offer {VendorOfferId}", request.VendorOfferId);
            return Result<CartItemDTO>.Failure(Error.NotFound("Inventory for this offer not found."), 404);
        }

        // 6) Verify there is sufficient stock
        var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;
        if (availableQuantity < request.Quantity)
        {
            _logger.LogWarning("Add cart item failed: insufficient stock for offer {VendorOfferId}. Available: {Available}, Requested: {Requested}", request.VendorOfferId, availableQuantity, request.Quantity);
            return Result<CartItemDTO>.Failure(Error.Validation($"Insufficient stock. Only {availableQuantity} unit(s) available."), 400);
        }

        // 7) Add or update the CartItem (prevent duplicate rows)
        var existingItem = await _unitOfWork.CartItems.GetCartItemByVendorOfferAsync(cartSession.Id, request.VendorOfferId);
        CartItem cartItem;

        if (existingItem is not null)
        {
            _logger.LogDebug("Updating existing cart item for offer {VendorOfferId} in cart {CartSessionId}", request.VendorOfferId, cartSession.Id);
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
        _logger.LogInformation("Cart item added/updated for cart {CartSessionId}, offer {VendorOfferId}", cartSession.Id, request.VendorOfferId);
        return Result<CartItemDTO>.Success(dto);
    }
}
