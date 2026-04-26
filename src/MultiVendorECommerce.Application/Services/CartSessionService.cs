using AutoMapper;
using MultiVendorECommerce.Application.DTOs.CartSession;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Services;

public class CartSessionService(IUnitOfWork unitOfWork, IMapper mapper) : ICartSessionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IEnumerable<CartSessionDTO>>> GetAllAsync()
    {
        var sessions = await _unitOfWork.CartSessions.GetAllAsync();
        return Result<IEnumerable<CartSessionDTO>>.Success(_mapper.Map<IEnumerable<CartSessionDTO>>(sessions));
    }

    public async Task<Result<CartSessionDTO>> GetByIdAsync(Guid id)
    {
        var session = await _unitOfWork.CartSessions.GetCartWithItemsAsync(id);
        if (session is null)
            return Result<CartSessionDTO>.Failure(Error.NotFound("Cart session not found."), 404);

        return Result<CartSessionDTO>.Success(_mapper.Map<CartSessionDTO>(session));
    }

    public async Task<Result<CartSessionDTO>> GetMyCartAsync(Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CartSessionDTO>.Failure(Error.Forbidden("Only customers can view their cart."), 403);

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
            return Result<CartSessionDTO>.Failure(Error.NotFound("Cart session not found."), 404);

        var cartWithItems = await _unitOfWork.CartSessions.GetCartWithItemsAsync(cartSession.Id);
        return Result<CartSessionDTO>.Success(_mapper.Map<CartSessionDTO>(cartWithItems!));
    }

    public async Task<Result<CartSessionDTO>> CreateAsync(Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result<CartSessionDTO>.Failure(Error.Forbidden("Only customers can create a cart session."), 403);

        var existing = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (existing is not null)
            return Result<CartSessionDTO>.Failure(Error.Validation("A cart session already exists for this customer."), 400);

        var session = new CartSession
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Result<CartSessionDTO>.Success(_mapper.Map<CartSessionDTO>(session), 201);
    }

    public async Task<Result> DeleteAsync(Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(userId);
        if (customer is null)
            return Result.Failure(Error.Forbidden("Only customers can delete their cart session."), 403);

        var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
        if (cartSession is null)
            return Result.Failure(Error.NotFound("Cart session not found."), 404);

        await _unitOfWork.CartSessions.DeleteAsync(cartSession);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(204);
    }
}
