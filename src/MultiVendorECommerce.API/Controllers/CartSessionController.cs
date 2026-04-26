using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.CartSession;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartSessionController(ICartSessionService cartSessionService, IAppLogger<CartSessionController> logger) : ControllerBase
{
    protected readonly ICartSessionService _cartSessionService = cartSessionService;
    protected readonly IAppLogger<CartSessionController> _logger = logger;

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<IEnumerable<CartSessionDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<CartSessionDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll cart sessions called");
        var result = await _cartSessionService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<CartSessionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CartSessionDTO>>> GetById(Guid id)
    {
        _logger.LogInformation("GetById cart session called with id {Id}", id);
        var result = await _cartSessionService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CartSessionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CartSessionDTO>>> GetMyCart()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("GetMyCart called for user {UserId}", userId);
        var result = await _cartSessionService.GetMyCartAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CartSessionDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<CartSessionDTO>>> Create()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Create cart session called for user {UserId}", userId);
        var result = await _cartSessionService.CreateAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("my")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> DeleteMyCart()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("DeleteMyCart called for user {UserId}", userId);
        var result = await _cartSessionService.DeleteAsync(userId);
        return StatusCode(result.StatusCode, result);
    }
}
