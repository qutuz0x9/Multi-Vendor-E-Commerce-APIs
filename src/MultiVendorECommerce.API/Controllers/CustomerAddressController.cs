using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Extensions;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.CustomerAddress;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Domain.Enums;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerAddressController(ICustomerAddressService customerAddressService, IAppLogger<CustomerAddressController> logger) : ControllerBase
{
    protected readonly ICustomerAddressService _customerAddressService = customerAddressService;
    protected readonly IAppLogger<CustomerAddressController> _logger = logger;

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<IEnumerable<CustomerAddressDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<CustomerAddressDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll customer addresses called");
        var result = await _customerAddressService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<CustomerAddressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CustomerAddressDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById customer address called with id {Id}", id);
        var result = await _customerAddressService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("type/{addressType}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(Result<IEnumerable<CustomerAddressDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<CustomerAddressDTO>>>> GetByType(CustomerAddressType addressType)
    {
        _logger.LogInformation("GetByType customer addresses called with type {AddressType}", addressType);
        var result = await _customerAddressService.GetByTypeAsync(addressType);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<IEnumerable<CustomerAddressDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<CustomerAddressDTO>>>> GetMyAddresses()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("GetMyAddresses called for user {UserId}", userId);
        var result = await _customerAddressService.GetMyAddressesAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CustomerAddressDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<CustomerAddressDTO>>> Create([FromBody] CreateCustomerAddressDTO request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Create customer address called for user {UserId}", userId);
        var result = await _customerAddressService.CreateAsync(userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result<CustomerAddressDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<CustomerAddressDTO>>> Update(int id, [FromBody] UpdateCustomerAddressDTO request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Update customer address called with id {Id} for user {UserId}", id, userId);
        var result = await _customerAddressService.UpdateAsync(id, userId, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Delete customer address called with id {Id} for user {UserId}", id, userId);
        var result = await _customerAddressService.DeleteAsync(id, userId);
        return StatusCode(result.StatusCode, result);
    }
}
