using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.Application.DTOs.Product;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    protected readonly IProductService _productService = productService;

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<ProductDTO>>> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("brand/{brandId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetByBrand(int brandId)
    {
        var result = await _productService.GetProductsByBrandAsync(brandId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<ActionResult<Result<IEnumerable<ProductDTO>>>> GetByCategory(int categoryId)
    {
        var result = await _productService.GetProductsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<ProductDTO>>> Create([FromBody] CreateProductDTO request)
    {
        var result = await _productService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<ProductDTO>>> Update(int id, [FromBody] UpdateProductDTO request)
    {
        var result = await _productService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
