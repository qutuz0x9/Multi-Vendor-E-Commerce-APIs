using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Helpers;
using MultiVendorECommerce.Shared.Results;
using MultiVendorECommerce.Shared.Utils;



namespace MultiVendorECommerce.Application.Services;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITokenService tokenService,
    ICookieService cookieService
    )
 : IAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICookieService _cookieService = cookieService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;


    public async Task<Result<RegisterResponseDTO>> RegisterUser(RegisterUserDTO request)
    {
        // 1) Use Common Register Service
        var registerResult = await Register(request);
        if (registerResult.IsFailure)
            return Result<RegisterResponseDTO>.Failure(registerResult.Errors, registerResult.StatusCode);

        var user = registerResult.Value!;

        // 2) Assign Customer Role For this User
        if (!await _roleManager.RoleExistsAsync(Roles.Customer))
        {
            await _roleManager.CreateAsync(new Role { Name = Roles.Customer });
        }
        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Customer);
        if (!roleResult.Succeeded)
            return Result<RegisterResponseDTO>.Failure(Error.Failure("Failed to assign role to user."), 500);

        // 3) Generate Token For This User
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);
        await CreateCustomerProfile(user, request);
        var response = new RegisterResponseDTO
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Role = Roles.Customer,
            Token = token
        };
        // 4) Return Result
        return Result<RegisterResponseDTO>.Success(response);
    }

    public async Task<Result<RegisterResponseDTO>> RegisterVendor(RegisterVendorDTO request)
    {
        // 1) Use Common Register Service
        var registerResult = await Register(request);
        if (registerResult.IsFailure)
            return Result<RegisterResponseDTO>.Failure(registerResult.Errors, registerResult.StatusCode);

        var user = registerResult.Value!;

        // 2) Assign Vendor Role For this User
        if (!await _roleManager.RoleExistsAsync(Roles.Vendor))
        {
            await _roleManager.CreateAsync(new Role { Name = Roles.Vendor });
        }
        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Vendor);
        if (!roleResult.Succeeded)
            return Result<RegisterResponseDTO>.Failure(Error.Failure("Failed to assign role to user."), 500);

        // 3) Generate Token For This User
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);
        await CreateVendorProfile(user, request);
        var response = new RegisterResponseDTO
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Role = Roles.Vendor,
            Token = token
        };
        // 4) Return Result
        return Result<RegisterResponseDTO>.Success(response);
    }



    /// <summary>
    /// The function `Register` takes a `RegisterUserDTO` request, maps it to a `User`, creates a user
    /// with the request data using `_userManager`, and returns the created user.
    /// </summary>
    /// <param name="RegisterUserDTO">RegisterUserDTO is a data transfer object (DTO) that contains the
    /// information needed to register a new user. It likely includes properties such as username,
    /// email, password, and any other relevant user details.</param>
    /// <returns>
    /// The method `Register` is returning a `Task<User>`, which is an asynchronous task that will
    /// eventually produce a `User` object.
    /// </returns>
    private async Task<Result<User>> Register(RegisterUserDTO request)
    {
        // 1) Check for duplicate email
        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
            return Result<User>.Failure(Error.Validation("Email is already in use."), 400);

        // 2) Check for duplicate username
        var existingByName = await _userManager.FindByNameAsync(request.Username);
        if (existingByName is not null)
            return Result<User>.Failure(Error.Validation("Username is already taken."), 400);

        // 3) Map Request To User
        var user = _mapper.Map<User>(request);
        user.Id = Guid.NewGuid();

        // 4) Create User With The Request Data
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Failure(e.Description));
            return Result<User>.Failure(errors, 500);
        }
        return Result<User>.Success(user);
    }

    private async Task CreateCustomerProfile(User user, RegisterUserDTO request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = request.Username,
            LastName = request.Username
        };
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
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
        await _unitOfWork.SaveChangesAsync();
    }

}
