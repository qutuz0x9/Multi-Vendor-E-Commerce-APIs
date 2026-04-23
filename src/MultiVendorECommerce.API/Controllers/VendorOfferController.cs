using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class OfferController(IVendorOfferService vendorOfferService) : ControllerBase
{
    protected readonly IVendorOfferService _vendorOfferService = vendorOfferService;

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<VendorOfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<VendorOfferDTO>>> GetById(int id)
    {
        var result = await _vendorOfferService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my-offers")]
    [ProducesResponseType(typeof(Result<IEnumerable<VendorOfferDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.Vendor)]
    public async Task<ActionResult<Result<IEnumerable<VendorOfferDTO>>>> GetMyOffers()
    {
        var userId = User.GetUserId();
        var result = await _vendorOfferService.GetOffersByVendorAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(Result<IEnumerable<VendorOfferDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<IEnumerable<VendorOfferDTO>>>> GetOffersByProduct(int productId)
    {
        var result = await _vendorOfferService.GetOffersByProductAsync(productId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<VendorOfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.Vendor)]
    public async Task<ActionResult<Result<VendorOfferDTO>>> Create([FromBody] CreateVendorOfferDTO request)
    {
        var userId = User.GetUserId();
        var result = await _vendorOfferService.CreateAsync(userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(Result<VendorOfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.Vendor)]
    public async Task<ActionResult<Result<VendorOfferDTO>>> Update(int id, [FromBody] UpdateVendorOfferDTO request)
    {
        var userId = User.GetUserId();
        var result = await _vendorOfferService.UpdateAsync(userId, id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = Roles.Vendor)]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var result = await _vendorOfferService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}