using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.VendorOffer;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class OfferController(IVendorOfferService vendorOfferService, IAppLogger<OfferController> logger) : ControllerBase
{
    protected readonly IVendorOfferService _vendorOfferService = vendorOfferService;
    protected readonly IAppLogger<OfferController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<VendorOfferDTO>>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<IEnumerable<VendorOfferDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll vendor offers called");
        var result = await _vendorOfferService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<VendorOfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<VendorOfferDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById vendor offer called with id {Id}", id);
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
        _logger.LogInformation("GetMyOffers called for vendor {UserId}", userId);
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
        _logger.LogInformation("GetOffersByProduct called with productId {ProductId}", productId);
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
        _logger.LogInformation("Create vendor offer called by vendor {UserId}", userId);
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
        _logger.LogInformation("Update vendor offer called with id {Id} by vendor {UserId}", id, userId);
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
        _logger.LogInformation("Delete vendor offer called with id {Id}", id);
        var result = await _vendorOfferService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}