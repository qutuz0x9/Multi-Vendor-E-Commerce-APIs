using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class OrderService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<Result<OrderDTO>> CreateOrderAsync(Guid userId)
    {
        // Phase 1: Validation (before transaction)

        // 1) Resolve Customer profile
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<OrderDTO>.Failure(Error.Forbidden("Only customers can place orders."), 403);

        // 2) Verify cart session exists
        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
            return Result<OrderDTO>.Failure(Error.NotFound("Cart session not found for this customer."), 404);

        // 3) Load cart items and ensure cart is not empty
        var cartItems = (await _unitOfWork.CartItems.GetItemsByCartAsync(cartSession.Id)).ToList();
        if (cartItems.Count == 0)
            return Result<OrderDTO>.Failure(Error.Validation("Your cart is empty."), 400);

        // 4) Validate each cart item: offer active, inventory sufficient; collect data for Phase 2
        var lineItems = new List<(CartItem CartItem, VendorOffer Offer, Inventory Inventory, Product Product)>();

        foreach (var cartItem in cartItems)
        {
            var offer = await _unitOfWork.VendorOffers.GetByIdAsync(cartItem.VendorOfferId);
            if (offer is null)
                return Result<OrderDTO>.Failure(Error.NotFound($"Vendor offer {cartItem.VendorOfferId} not found."), 404);

            if (offer.Staus != VendorOfferStatus.Active)
                return Result<OrderDTO>.Failure(Error.Validation($"Offer {offer.Id} is not currently available."), 400);

            var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(offer.Id);
            if (inventory is null)
                return Result<OrderDTO>.Failure(Error.NotFound($"Inventory for offer {offer.Id} not found."), 404);

            var availableQty = inventory.Quantity - inventory.ReservedQuantity;
            if (availableQty < cartItem.Quantity)
                return Result<OrderDTO>.Failure(
                    Error.Validation($"Insufficient stock for offer {offer.Id}. Available: {availableQty}, Requested: {cartItem.Quantity}."), 400);

            var product = await _unitOfWork.Products.GetByIdAsync(offer.ProductId);
            if (product is null)
                return Result<OrderDTO>.Failure(Error.NotFound($"Product {offer.ProductId} not found."), 404);

            lineItems.Add((cartItem, offer, inventory, product));
        }

        // 5) Resolve shipping address (first Shipping-type address)
        var shippingAddresses = await _unitOfWork.CustomerAddresses.GetAddressesByTypeAsync(
            customer.Id, (int)CustomerAddressType.Shipping);
        var shippingAddr = shippingAddresses.FirstOrDefault();
        if (shippingAddr is null)
            return Result<OrderDTO>.Failure(Error.Validation("No shipping address on file. Please add a shipping address before placing an order."), 400);

        // 6) Fetch user for phone number
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var phoneNumber = user?.PhoneNumber ?? string.Empty;

        // Phase 2: Order Creation (inside a single DB transaction)

        await _unitOfWork.BeginTransactionAsync();

        // 7) Create the Order shell
        var order = new Order
        {
            CustomerId = customer.Id,
            TotalAmount = 0,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Orders.AddAsync(order);

        // Intermediate save to materialise order.Id (still within open transaction)
        await _unitOfWork.SaveChangesAsync();

        // 8) Create OrderItems and reserve inventory
        var orderItems = new List<OrderItem>();

        foreach (var (cartItem, offer, inventory, product) in lineItems)
        {
            var unitPrice = offer.Price;
            var lineTotal = unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                VendorOfferId = offer.Id,
                ProductName = product.Name,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                Price = lineTotal,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.OrderItems.AddAsync(orderItem);
            orderItems.Add(orderItem);

            // Reserve stock to prevent double-sell
            inventory.ReservedQuantity += cartItem.Quantity;
            await _unitOfWork.Inventories.UpdateAsync(inventory);
        }

        // 9) Set the computed total on the order
        order.TotalAmount = orderItems.Sum(oi => oi.Price ?? 0);
        await _unitOfWork.Orders.UpdateAsync(order);

        // 10) Create the shipping address snapshot
        var orderShippingAddress = new OrderShippingAddress
        {
            OrderId = order.Id,
            ShippingAddress = shippingAddr.Address,
            ShippingCity = shippingAddr.City,
            ShippingCountry = shippingAddr.Country,
            ShippingPhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.OrderShippingAddresses.AddAsync(orderShippingAddress);

        // Commit saves all remaining changes and commits the transaction
        await _unitOfWork.CommitTransactionAsync();

        // Phase 3: Build and return response DTO
        var orderDto = _mapper.Map<OrderDTO>(order);
        orderDto.OrderItems = _mapper.Map<IEnumerable<OrderItemDTO>>(orderItems);
        orderDto.ShippingAddress = _mapper.Map<OrderShippingAddressDTO>(orderShippingAddress);

        return Result<OrderDTO>.Success(orderDto, 201);
    }
}
