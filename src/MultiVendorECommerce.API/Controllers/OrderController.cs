using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.Application.DTOs.Order;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.Customer)]
public class OrderController(IOrderService orderService) : ControllerBase
{
    protected readonly IOrderService _orderService = orderService;

    [HttpPost]
    [ProducesResponseType(typeof(Result<OrderDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<OrderDTO>>> CreateOrder()
    {
        var userId = User.GetUserId();
        var result = await _orderService.CreateOrderAsync(userId);
        return StatusCode(result.StatusCode, result);
    }
}
