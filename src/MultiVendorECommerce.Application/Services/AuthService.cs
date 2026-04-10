using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;
using Npgsql.Replication;

namespace MultiVendorECommerce.Application.Services;

public class AuthService(
    UserManager<User> userManager, RoleManager<Role> roleManager, IMapper mapper)
 : IAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;


    public async Task<Result<RegisterResponseDTO>> RegisterUser(RegisterUserDTO request)
    {
        // 1) Use Common Register Service
        var user = await Register(request);
        // 2) Assign Customer Role For this User
        if (!await _roleManager.RoleExistsAsync(Roles.Customer))
        {
            await _roleManager.CreateAsync(new Role { Name = Roles.Customer });
        }
        await _userManager.AddToRoleAsync(user, Roles.Customer);
        var response = _mapper.Map<RegisterResponseDTO>(user);
        // 3) Return Result 
        return Result<RegisterResponseDTO>.Success(response);

    }

    // public async Task<Result<RegisterResponseDTO>> RegisterVendor(RegisterVendorDTO request)
    // {
    //     // 1) Use Common Register Service
    //     var vendor = await Register(request);
    //     // 2) Assign Vendor Role For this User


    //     // 3) Return Result
    //     return Result<RegisterResponseDTO>.Success();
    // }



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
    private async Task<User> Register(RegisterUserDTO request)
    {
        // 1) Map Request To User 
        var user = _mapper.Map<User>(request);

        // 2) Create User With The Request Data
        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded == false)
        {
            throw new Exception("Failed to Create User");
        }
        return user; // Only Return User, The Role Assignment and Response Mapping will be handled in the calling method
    }
}
