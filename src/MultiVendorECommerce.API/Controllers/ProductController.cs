using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService, IAppLogger<ProductController> logger) : ControllerBase
{
    protected readonly IProductService _productService = productService;
    protected readonly IAppLogger<ProductController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<ProductDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll products called");
        var result = await _productService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<ProductDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById product called with id {Id}", id);
        var result = await _productService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("brand/{brandId:int}")]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetByBrand(int brandId)
    {
        _logger.LogInformation("GetByBrand products called with brandId {BrandId}", brandId);
        var result = await _productService.GetProductsByBrandAsync(brandId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetByCategory(int categoryId)
    {
        _logger.LogInformation("GetByCategory products called with categoryId {CategoryId}", categoryId);
        var result = await _productService.GetProductsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<ProductDTO>>> Create([FromBody] CreateProductDTO request)
    {
        _logger.LogInformation("Create product called");
        var result = await _productService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<Result<ProductDTO>>> Update(int id, [FromBody] UpdateProductDTO request)
    {
        _logger.LogInformation("Update product called with id {Id}", id);
        var result = await _productService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(Result<ProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        _logger.LogInformation("Delete product called with id {Id}", id);
        var result = await _productService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
