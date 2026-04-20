using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiVendorECommerce.Application.DTOs.Category;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    protected readonly ICategoryService _categoryService = categoryService;

    /// <summary>
    /// This C# function uses HTTP GET to retrieve all categories, with specific response types and
    /// authorization roles defined.
    /// </summary>
    /// <returns>
    /// The `GetAll` method is returning an `ActionResult` with a generic type of
    /// `Result<IEnumerable<CategoryDTO>>`. This method is marked with various `ProducesResponseType`
    /// attributes to specify the possible HTTP status codes and response types that can be returned. The
    /// method is also decorated with an `Authorize` attribute specifying that only users with roles "Admin"
    /// or "Vendor" are allowed to access this endpoint.
    /// </returns>
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
        var result = await _categoryService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// This C# function uses HTTP GET to retrieve a category by ID, with specific response types and
    /// authorization roles.
    /// </summary>
    /// <param name="id">The `id` parameter in the `HttpGet` attribute specifies that the method should
    /// only respond to HTTP GET requests where the `id` parameter is an integer value.</param>
    /// <returns>
    /// The GetById method is returning an ActionResult with a Result object of type CategoryDTO. The
    /// method is decorated with various attributes such as HttpGet, Authorize, and ProducesResponseType
    /// to define the behavior and response types for different HTTP status codes. The method retrieves a
    /// category by its ID asynchronously and returns the result with the corresponding status code.
    /// </returns>
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
        var result = await _categoryService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// The above function is an HTTP POST endpoint in C# that creates a new category, requires
    /// authorization as an Admin, and returns a specific result based on the status code.
    /// </summary>
    /// <param name="CreateCategoryDTO">`CreateCategoryDTO` is a data transfer object (DTO) that represents
    /// the data required to create a new category. It is used as the input parameter for the `Create`
    /// method in the controller.</param>
    /// <returns>
    /// The `Create` method is returning an `ActionResult` with a generic type of `Result<CategoryDTO>`. The
    /// result is obtained from the `_categoryService.CreateAsync(request)` method and is returned with the
    /// corresponding status code specified in the `result.StatusCode`.
    /// </returns>
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
        var result = await _categoryService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// This C# function updates a category with the specified ID and returns a result based on the
    /// operation's status code.
    /// </summary>
    /// <param name="id">The `id` parameter in the `Update` method is of type `int` and is used to
    /// identify the specific category that needs to be updated.</param>
    /// <param name="UpdateCategoryDTO">The `UpdateCategoryDTO` is a data transfer object (DTO) used for
    /// updating a category. It likely contains properties that represent the data needed to update a
    /// category, such as the category name, description, or any other relevant information that can be
    /// modified.</param>
    /// <returns>
    /// The Update method is returning an ActionResult with a Result object containing a CategoryDTO. The
    /// method can return different HTTP status codes based on the outcome of the operation:
    /// </returns>
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
        var result = await _categoryService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// This C# function uses HTTP DELETE method to delete a resource by ID, with authorization for Admin
    /// role and returns appropriate status codes based on the result.
    /// </summary>
    /// <param name="id">The `id` parameter in the `Delete` method is of type `int` and represents the
    /// identifier of the resource that needs to be deleted.</param>
    /// <returns>
    /// The Delete method is returning an ActionResult of type Result. The Result object contains
    /// information about the status code and any additional data related to the deletion operation. The
    /// method is also decorated with various attributes to specify the expected HTTP status codes for
    /// different scenarios such as success, internal server error, bad request, not found, unauthorized,
    /// and forbidden. Additionally, the method is restricted to users with the "Admin"
    /// </returns>
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
        var result = await _categoryService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
