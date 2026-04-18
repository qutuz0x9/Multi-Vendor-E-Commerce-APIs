using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.Application.DTOs.ProductCategory;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductCategoryController(IProductCategoryService productCategoryService) : ControllerBase
{
    protected readonly IProductCategoryService _productCategoryService = productCategoryService;

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductCategoryDTO>>>> GetCategoriesByProduct(int productId)
    {
        var result = await _productCategoryService.GetCategoriesByProductAsync(productId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductCategoryDTO>>>> GetProductsByCategory(int categoryId)
    {
        var result = await _productCategoryService.GetProductsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<ProductCategoryDTO>>> AddProductToCategory([FromBody] CreateProductCategoryDTO request)
    {
        var result = await _productCategoryService.AddProductToCategoryAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> RemoveProductFromCategory(int id)
    {
        var result = await _productCategoryService.RemoveProductFromCategoryAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
