using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;


namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IAppLogger<AuthController> logger) : ControllerBase
{
    protected readonly IAuthService _authService = authService;
    protected readonly IAppLogger<AuthController> _logger = logger;


    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> RegisterCustomer([FromBody] RegisterUserDTO request)
    {
        _logger.LogInformation("RegisterCustomer called for email {Email}", request.Email);
        var result = await _authService.RegisterUser(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("vendor/register")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> RegisterVendor([FromBody] RegisterVendorDTO request)
    {
        _logger.LogInformation("RegisterVendor called for email {Email}", request.Email);
        var result = await _authService.RegisterVendor(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDTO>>> Login([FromBody] LoginRequestDTO request)
    {
        _logger.LogInformation("Login called for email {Email}", request.Email);
        var result = await _authService.Login(request);
        return StatusCode(result.StatusCode, result);
    }
}

