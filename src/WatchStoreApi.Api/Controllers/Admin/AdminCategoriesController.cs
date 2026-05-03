using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.DTOs.Categories;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers.Admin;

[Route("api/admin/categories")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoryService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? Created(string.Empty, new { id = result.Value })
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoryService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }
}
