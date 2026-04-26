using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService, IAppLogger<CategoryController> logger) : ControllerBase
{
    protected readonly ICategoryService _categoryService = categoryService;
    protected readonly IAppLogger<CategoryController> _logger = logger;


    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<CategoryDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<ActionResult<Result<IEnumerable<CategoryDTO>>>> GetAll()
    {
        _logger.LogInformation("GetAll categories called");
        var result = await _categoryService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Result<CategoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<ActionResult<Result<CategoryDTO>>> GetById(int id)
    {
        _logger.LogInformation("GetById category called with id {Id}", id);
        var result = await _categoryService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPost]
    [ProducesResponseType(typeof(Result<CategoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<Result<CategoryDTO>>> Create([FromBody] CreateCategoryDTO request)
    {
        _logger.LogInformation("Create category called");
        var result = await _categoryService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }


    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(Result<CategoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<CategoryDTO>>> Update(int id, [FromBody] UpdateCategoryDTO request)
    {
        _logger.LogInformation("Update category called with id {Id}", id);
        var result = await _categoryService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }


    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]

    public async Task<ActionResult<Result>> Delete(int id)
    {
        _logger.LogInformation("Delete category called with id {Id}", id);
        var result = await _categoryService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
