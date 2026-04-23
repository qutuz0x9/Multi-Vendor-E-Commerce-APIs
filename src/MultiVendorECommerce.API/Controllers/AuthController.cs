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

   
    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> RegisterCustomer([FromBody] RegisterUserDTO request)
    {
        var result = await _authService.RegisterUser(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("vendor/register")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> RegisterVendor([FromBody] RegisterVendorDTO request)
    {
        var result = await _authService.RegisterVendor(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> Login([FromBody] LoginRequestDTO request)
    {
        var result = await _authService.Login(request);
        return StatusCode(result.StatusCode, result);
    }
}

