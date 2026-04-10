using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;


namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    protected readonly IAuthService _authService = authService;

    /// <summary>
    /// This C# function registers a customer by calling an authentication service and returning the
    /// result with the corresponding status code.
    /// </summary>
    /// <param name="RegisterUserDTO">The RegisterUserDTO is a data transfer object that contains the
    /// information needed to register a new customer. It likely includes fields such as username, email,
    /// password, and any other required information for creating a new user account.</param>
    /// <returns>
    /// The method `RegisterCustomer` is returning a `Task` that will eventually yield an `ActionResult`
    /// containing a `Result` object of type `RegisterResponseDTO`.
    /// </returns>
    [HttpPost("register")]
    public async Task<ActionResult<Result<RegisterResponseDTO>>> RegisterCustomer([FromBody] RegisterUserDTO request)
    {
        var result = await _authService.RegisterUser(request);
        return StatusCode(result.StatusCode, result);
    }

    //[HttpPost("vendor/register")]
    //public async Task<ActionResult<Result<RegisterResponseDTO>>> RegisterVendor([FromBody] RegisterVendorDTO request)
    //{
    //    var result = await _authService.RegisterVendor(request);
    //    return StatusCode(result.StatusCode, result);
    //}
}
