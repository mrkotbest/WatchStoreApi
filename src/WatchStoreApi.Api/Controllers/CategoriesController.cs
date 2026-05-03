using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }
}
