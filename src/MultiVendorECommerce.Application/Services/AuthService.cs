using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Logging;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;

namespace MultiVendorECommerce.Application.Services;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITokenService tokenService,
    ICookieService cookieService,
    IAppLogger<AuthService> logger
    )
 : IAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICookieService _cookieService = cookieService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAppLogger<AuthService> _logger = logger;


    public async Task<Result<AuthResponseDTO>> RegisterUser(RegisterUserDTO request)
    {
        _logger.LogInformation("Registering customer with username {Username}", request.Username);
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1) Use Common Register Service
            var registerResult = await Register(request);
            if (registerResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Customer registration failed for username {Username}", request.Username);
                return Result<AuthResponseDTO>.Failure(registerResult.Errors, registerResult.StatusCode);
            }

            var user = registerResult.Value!;

            // 2) Assign Customer Role For this User
            var roleResult = await AssignRole(user, Roles.Customer);
            if (roleResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Customer registration failed: could not assign role for user {UserId}", user.Id);
                return Result<AuthResponseDTO>.Failure(roleResult.Errors, roleResult.StatusCode);
            }

            // 3) Create Customer Profile + Cart Session
            var cartSessionId = await CreateCustomerProfile(user, request);

            // 4) Generate Token For This User
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles, cartSessionId);

            //5) Generate Refresh Token and Set It In HttpOnly Cookie
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            _cookieService.SetCookie("refreshToken", refreshToken, 7);

            // 6) Commit Transaction
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Customer {UserId} registered successfully", user.Id);
            var response = new AuthResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Role = Roles.Customer,
                Token = token
            };
            return Result<AuthResponseDTO>.Success(response);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result<AuthResponseDTO>> RegisterVendor(RegisterVendorDTO request)
    {
        _logger.LogInformation("Registering vendor with username {Username}", request.Username);
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1) Use Common Register Service
            var registerResult = await Register(request);
            if (registerResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Vendor registration failed for username {Username}", request.Username);
                return Result<AuthResponseDTO>.Failure(registerResult.Errors, registerResult.StatusCode);
            }

            var user = registerResult.Value!;

            // 2) Assign Vendor Role For this User
            var roleResult = await AssignRole(user, Roles.Vendor);
            if (roleResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Vendor registration failed: could not assign role for user {UserId}", user.Id);
                return Result<AuthResponseDTO>.Failure(roleResult.Errors, roleResult.StatusCode);
            }

            // 3) Create Vendor Profile
            await CreateVendorProfile(user, request);

            // 4) Generate Token For This User
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles);

            //5) Generate Refresh Token and Set It In HttpOnly Cookie
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            _cookieService.SetCookie("refreshToken", refreshToken, 7);

            // 6) Commit Transaction
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Vendor {UserId} registered successfully", user.Id);
            var response = new AuthResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Role = Roles.Vendor,
                Token = token
            };
            return Result<AuthResponseDTO>.Success(response);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<Result> AssignRole(User user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new Role { Name = roleName });
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
            return Result.Failure(Error.Failure("Failed to assign role to user."), 500);

        return Result.Success();
    }

    private async Task<Result<User>> Register(RegisterUserDTO request)
    {
        // 1) Check for duplicate email
        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
        {
            _logger.LogWarning("Registration failed: email {Email} is already in use", request.Email);
            return Result<User>.Failure(Error.Validation("Email is already in use."), 400);
        }

        // 2) Check for duplicate username
        var existingByName = await _userManager.FindByNameAsync(request.Username);
        if (existingByName is not null)
        {
            _logger.LogWarning("Registration failed: username {Username} is already taken", request.Username);
            return Result<User>.Failure(Error.Validation("Username is already taken."), 400);
        }

        // 3) Map Request To User
        var user = _mapper.Map<User>(request);
        user.Id = Guid.NewGuid();

        // 4) Create User With The Request Data
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Registration failed: identity errors for username {Username}", request.Username);
            var errors = result.Errors.Select(e => Error.Failure(e.Description));
            return Result<User>.Failure(errors, 500);
        }
        return Result<User>.Success(user);
    }

    private async Task<Guid> CreateCustomerProfile(User user, RegisterUserDTO request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = request.Username,
            LastName = request.Username
        };
        await _unitOfWork.Customers.AddAsync(customer);

        // Create a Cart Session Linked To Customer 
        var cartSession = new CartSession
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.CartSessions.AddAsync(cartSession);

        return cartSession.Id;
    }

    private async Task CreateVendorProfile(User user, RegisterVendorDTO request)
    {
        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BusinessName = request.Username,
            WebsiteUrl = $"https://{request.Username.ToLower()}.com",
            Slug = SlugHelper.GenerateSlug(request.Username)
        };
        await _unitOfWork.Vendors.AddAsync(vendor);
    }

    public async Task<Result<AuthResponseDTO>> Login(LoginRequestDTO request)
    {
        _logger.LogInformation("Login attempt for email {Email}", request.Email);
        // 1) Find User By Email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
            return Result<AuthResponseDTO>.Failure(Error.Validation("Invalid email or password."), 400);
        }
        // 2) Check Password
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: invalid password for user {UserId}", user.Id);
            return Result<AuthResponseDTO>.Failure(Error.Validation("Invalid email or password."), 400);
        }
        // 4) Generate Token For This User
        var roles = await _userManager.GetRolesAsync(user);
        var customer = await _unitOfWork.Customers.GetCustomerByUserIdAsync(user.Id);
        Guid? cartSessionId = null;
        if (customer is not null)
        {
            var cartSession = await _unitOfWork.CartSessions.GetCartByCustomerAsync(customer.Id);
            cartSessionId = cartSession?.Id;
        }
        var token = _tokenService.GenerateAccessToken(user, roles, cartSessionId);
        // 5) Generate Refresh Token and Set It In HttpOnly Cookie
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        _cookieService.SetCookie("refreshToken", refreshToken, 7);
        // 6) Return Response
        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        var response = new AuthResponseDTO
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Token = token
        };
        return Result<AuthResponseDTO>.Success(response);
    }
}
