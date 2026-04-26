using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.API.Logging;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductCategoryController(IProductCategoryService productCategoryService, IAppLogger<ProductCategoryController> logger) : ControllerBase
{
    protected readonly IProductCategoryService _productCategoryService = productCategoryService;
    protected readonly IAppLogger<ProductCategoryController> _logger = logger;

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductCategoryDTO>>>> GetCategoriesByProduct(int productId)
    {
        _logger.LogInformation("GetCategoriesByProduct called with productId {ProductId}", productId);
        var result = await _productCategoryService.GetCategoriesByProductAsync(productId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductCategoryDTO>>>> GetProductsByCategory(int categoryId)
    {
        _logger.LogInformation("GetProductsByCategory called with categoryId {CategoryId}", categoryId);
        var result = await _productCategoryService.GetProductsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<ProductCategoryDTO>>> AddProductToCategory([FromBody] CreateProductCategoryDTO request)
    {
        _logger.LogInformation("AddProductToCategory called");
        var result = await _productCategoryService.AddProductToCategoryAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> RemoveProductFromCategory(int id)
    {
        _logger.LogInformation("RemoveProductFromCategory called with id {Id}", id);
        var result = await _productCategoryService.RemoveProductFromCategoryAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
