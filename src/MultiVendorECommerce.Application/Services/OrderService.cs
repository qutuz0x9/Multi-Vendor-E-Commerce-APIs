using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class OrderService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IAppLogger<OrderService> logger) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IAppLogger<OrderService> _logger = logger;

    public async Task<Result<IEnumerable<OrderDTO>>> GetAllOrdersAsync()
    {
        _logger.LogDebug("Fetching all orders");
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return Result<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders));
    }

    public async Task<Result<OrderDTO>> GetOrderByIdAsync(int orderId)
    {
        _logger.LogDebug("Fetching order {OrderId}", orderId);
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return Result<OrderDTO>.Failure(Error.NotFound("Order not found."), 404);
        }

        var orderDto = _mapper.Map<OrderDTO>(order);
        orderDto.OrderItems = _mapper.Map<IEnumerable<OrderItemDTO>>(order.OrderItems);
        orderDto.ShippingAddress = _mapper.Map<OrderShippingAddressDTO>(order.ShippingAddress);

        return Result<OrderDTO>.Success(orderDto);
    }

    public async Task<Result<IEnumerable<OrderDTO>>> GetMyOrdersAsync(Guid userId)
    {
        _logger.LogDebug("Fetching orders for user {UserId}", userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("GetMyOrders failed: no customer profile for user {UserId}", userId);
            return Result<IEnumerable<OrderDTO>>.Failure(Error.Forbidden("Only customers can view their orders."), 403);
        }

        var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(customer.Id);
        return Result<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders));
    }

    public async Task<Result> CancelOrderAsync(int orderId, Guid userId)
    {
        _logger.LogInformation("Cancelling order {OrderId} for user {UserId}", orderId, userId);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("Cancel order failed: no customer profile for user {UserId}", userId);
            return Result.Failure(Error.Forbidden("Only customers can cancel their orders."), 403);
        }

        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("Cancel order failed: order {OrderId} not found", orderId);
            return Result.Failure(Error.NotFound("Order not found."), 404);
        }

        if (order.CustomerId != customer.Id)
        {
            _logger.LogWarning("Cancel order forbidden: order {OrderId} does not belong to customer {CustomerId}", orderId, customer.Id);
            return Result.Failure(Error.Forbidden("You are not allowed to cancel this order."), 403);
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            _logger.LogWarning("Cancel order failed: order {OrderId} has status {OrderStatus} which cannot be cancelled", orderId, order.Status);
            return Result.Failure(Error.Validation($"Cannot cancel an order with status '{order.Status}'."), 400);
        }

        // Release reserved inventory for each order item
        foreach (var item in order.OrderItems)
        {
            var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(item.VendorOfferId);
            if (inventory is not null)
            {
                inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - item.Quantity);
                await _unitOfWork.Inventories.UpdateAsync(inventory);
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.ModifiedAt = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
        return Result.Success(200);
    }

    public async Task<Result<OrderDTO>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDTO request)
    {
        _logger.LogInformation("Updating status of order {OrderId} to {NewStatus}", orderId, request.Status);
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("UpdateOrderStatus failed: order {OrderId} not found", orderId);
            return Result<OrderDTO>.Failure(Error.NotFound("Order not found."), 404);
        }

        order.Status = request.Status;
        order.ModifiedAt = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var orderDto = _mapper.Map<OrderDTO>(order);
        orderDto.OrderItems = _mapper.Map<IEnumerable<OrderItemDTO>>(order.OrderItems);
        orderDto.ShippingAddress = _mapper.Map<OrderShippingAddressDTO>(order.ShippingAddress);

        _logger.LogInformation("Order {OrderId} status updated to {NewStatus}", orderId, request.Status);
        return Result<OrderDTO>.Success(orderDto);
    }

    public async Task<Result<OrderDTO>> CreateOrderAsync(Guid userId)
    {
        _logger.LogInformation("Creating order for user {UserId}", userId);
        // Phase 1: Validation (before transaction)

        // 1) Resolve Customer profile
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
        {
            _logger.LogWarning("CreateOrder failed: no customer profile for user {UserId}", userId);
            return Result<OrderDTO>.Failure(Error.Forbidden("Only customers can place orders."), 403);
        }

        // 2) Verify cart session exists
        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
        {
            _logger.LogWarning("CreateOrder failed: no cart session for customer {CustomerId}", customer.Id);
            return Result<OrderDTO>.Failure(Error.NotFound("Cart session not found for this customer."), 404);
        }

        // 3) Load cart items and ensure cart is not empty
        var cartItems = (await _unitOfWork.CartItems.GetItemsByCartAsync(cartSession.Id)).ToList();
        if (cartItems.Count == 0)
        {
            _logger.LogWarning("CreateOrder failed: cart is empty for customer {CustomerId}", customer.Id);
            return Result<OrderDTO>.Failure(Error.Validation("Your cart is empty."), 400);
        }

        // 4) Validate each cart item: offer active, inventory sufficient; collect data for Phase 2
        var lineItems = new List<(CartItem CartItem, VendorOffer Offer, Inventory Inventory, Product Product)>();

        foreach (var cartItem in cartItems)
        {
            var offer = await _unitOfWork.VendorOffers.GetByIdAsync(cartItem.VendorOfferId);
            if (offer is null)
            {
                _logger.LogWarning("CreateOrder failed: vendor offer {VendorOfferId} not found", cartItem.VendorOfferId);
                return Result<OrderDTO>.Failure(Error.NotFound($"Vendor offer {cartItem.VendorOfferId} not found."), 404);
            }

            if (offer.Staus != VendorOfferStatus.Active)
            {
                _logger.LogWarning("CreateOrder failed: offer {OfferId} is not active", offer.Id);
                return Result<OrderDTO>.Failure(Error.Validation($"Offer {offer.Id} is not currently available."), 400);
            }

            var inventory = await _unitOfWork.Inventories.GetInventoryByVendorOfferAsync(offer.Id);
            if (inventory is null)
            {
                _logger.LogWarning("CreateOrder failed: no inventory for offer {OfferId}", offer.Id);
                return Result<OrderDTO>.Failure(Error.NotFound($"Inventory for offer {offer.Id} not found."), 404);
            }

            var availableQty = inventory.Quantity - inventory.ReservedQuantity;
            if (availableQty < cartItem.Quantity)
            {
                _logger.LogWarning("CreateOrder failed: insufficient stock for offer {OfferId}. Available: {Available}, Requested: {Requested}", offer.Id, availableQty, cartItem.Quantity);
                return Result<OrderDTO>.Failure(
                    Error.Validation($"Insufficient stock for offer {offer.Id}. Available: {availableQty}, Requested: {cartItem.Quantity}."), 400);
            }

            var product = await _unitOfWork.Products.GetByIdAsync(offer.ProductId);
            if (product is null)
            {
                _logger.LogWarning("CreateOrder failed: product {ProductId} not found", offer.ProductId);
                return Result<OrderDTO>.Failure(Error.NotFound($"Product {offer.ProductId} not found."), 404);
            }

            lineItems.Add((cartItem, offer, inventory, product));
        }

        // 5) Resolve shipping address (first Shipping-type address)
        var shippingAddresses = await _unitOfWork.CustomerAddresses.GetAddressesByTypeAsync(
            customer.Id, (int)CustomerAddressType.Shipping);
        var shippingAddr = shippingAddresses.FirstOrDefault();
        if (shippingAddr is null)
        {
            _logger.LogWarning("CreateOrder failed: no shipping address for customer {CustomerId}", customer.Id);
            return Result<OrderDTO>.Failure(Error.Validation("No shipping address on file. Please add a shipping address before placing an order."), 400);
        }

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

        _logger.LogInformation("Order {OrderId} created for customer {CustomerId} with total {TotalAmount}", order.Id, customer.Id, order.TotalAmount);
        return Result<OrderDTO>.Success(orderDto, 201);
    }
}
