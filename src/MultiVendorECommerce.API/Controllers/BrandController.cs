using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.Brand;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandController(IBrandService brandService, IAppLogger<BrandController> logger) : ControllerBase
{
    protected readonly IBrandService _brandService = brandService;
    protected readonly IAppLogger<BrandController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<BrandDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<ActionResult<Result<IEnumerable<BrandDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll brands called");
        var result = await _brandService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<BrandDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<ActionResult<Result<BrandDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById brand called with id {Id}", id);
        var result = await _brandService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<BrandDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<BrandDTO>>> Create([FromBody] CreateBrandDTO request)
    {
        _logger.LogInformation("Create brand called");
        var result = await _brandService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(Result<BrandDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<BrandDTO>>> Update(int id, [FromBody] UpdateBrandDTO request)
    {
        _logger.LogInformation("Update brand called with id {Id}", id);
        var result = await _brandService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(Result<BrandDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        _logger.LogInformation("Delete brand called with id {Id}", id);
        var result = await _brandService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
