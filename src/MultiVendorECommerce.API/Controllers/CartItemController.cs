using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.CartItem;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartItemController(ICartItemService cartItemService, IAppLogger<CartItemController> logger) : ControllerBase
{
    protected readonly ICartItemService _cartItemService = cartItemService;
    protected readonly IAppLogger<CartItemController> _logger = logger;

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<IEnumerable<CartItemDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<CartItemDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll cart items called");
        var result = await _cartItemService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<CartItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CartItemDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById cart item called with id {Id}", id);
        var result = await _cartItemService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<IEnumerable<CartItemDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IEnumerable<CartItemDTO>>>> GetMyCartItems()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("GetMyCartItems called for user {UserId}", userId);
        var result = await _cartItemService.GetMyCartItemsAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CartItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CartItemDTO>>> AddItem([FromBody] AddCartItemDTO request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("AddItem called for user {UserId}", userId);
        var result = await _cartItemService.AddItemAsync(userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CartItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CartItemDTO>>> UpdateItem(int id, [FromBody] UpdateCartItemDTO request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("UpdateItem called for cart item {Id} by user {UserId}", id, userId);
        var result = await _cartItemService.UpdateAsync(id, userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> RemoveItem(int id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("RemoveItem called for cart item {Id} by user {UserId}", id, userId);
        var result = await _cartItemService.RemoveItemAsync(id, userId);
        return StatusCode(result.StatusCode, result);
    }
}
